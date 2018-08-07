using Lykke.HttpClientGenerator.Infrastructure;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Lykke.HttpClientGenerator.Caching
{
    /// <summary>
    /// Adds caching to method calls.
    /// </summary>
    public class CachingCallsWrapper : ICallsWrapper
    {
        private readonly IRemovableAsyncCacheProvider _asyncCacheProvider;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly ConcurrentDictionary<string, object> _cacheKeys;
        //ReaderWriterLock works best where most accesses are reads, while writes are infrequent and of short duration.
        //Multiple readers alternate with single writers, so that neither readers nor writers are blocked for long periods.
        private readonly AsyncReaderWriterLock _readerWriterLock = new AsyncReaderWriterLock();

        /// <inheritdoc />
        public CachingCallsWrapper(ICachingStrategy cachingStrategy, IRemovableAsyncCacheProvider asyncCacheProvider)
        {
            _cacheKeys = new ConcurrentDictionary<string, object>();
            _asyncCacheProvider = asyncCacheProvider;
            _cachingStrategy = cachingStrategy;
            _retryPolicy = Policy
                .CacheAsync(_asyncCacheProvider,
                    new ContextualTtl());
        }

        /// <inheritdoc />
        public async Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler)
        {
            var cachingTime = _cachingStrategy.GetCachingTime(targetMethod, args);

            if (cachingTime <= TimeSpan.Zero)
                return await innerHandler();

            var contextData = new Dictionary<string, object>
            {
                {ContextualTtl.TimeSpanKey, cachingTime}
            };

            string cacheKey = GetExecutionKey(targetMethod, args);

            using (var readLock = await _readerWriterLock.ReaderLockAsync())
            {
                _cacheKeys[cacheKey] = true;
                return await _retryPolicy.ExecuteAsync(async (context, ct) => await innerHandler(),
                    new Context(cacheKey, contextData), default);
            }
        }

        public async Task InvalidateCache()
        {
            var cts = new CancellationTokenSource();

            using (var writeLock = await _readerWriterLock.WriterLockAsync())
            {
                var clearCacheKeys = _cacheKeys.Keys.ToArray();

                foreach (var cacheKey in clearCacheKeys)
                {
                    await _asyncCacheProvider.RemoveAsync(cacheKey, cts.Token);
                    _cacheKeys.TryRemove(cacheKey, out var x);
                }
            }
        }

        /// <summary>
        /// Gets the amount of time for which the result of <paramref name="targetMethod"/>
        /// executed with <paramref name="args"/> will be cached.
        /// </summary>
        private static string GetExecutionKey(MethodInfo targetMethod, object[] args)
        {
            return $"{targetMethod.DeclaringType}:{targetMethod.Name}:{targetMethod.GetHashCode()}:{JsonConvert.SerializeObject(args)}";
        }
    }
}