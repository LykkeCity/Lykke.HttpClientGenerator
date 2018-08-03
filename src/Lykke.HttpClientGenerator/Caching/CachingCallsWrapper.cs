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

namespace Lykke.HttpClientGenerator.Caching
{
    /// <summary>
    /// Adds caching to method calls.
    /// </summary>
    public class CachingCallsWrapper : ICallsWrapper
    {
        private readonly ICustomAsyncCacheProvider _asyncCacheProvider;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly ConcurrentDictionary<string, object> _cacheKeys;
        //ReaderWriterLock works best where most accesses are reads, while writes are infrequent and of short duration.
        //Multiple readers alternate with single writers, so that neither readers nor writers are blocked for long periods.
        //https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlock(v=vs.110).aspx
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        /// <inheritdoc />
        public CachingCallsWrapper(ICachingStrategy cachingStrategy, ICustomAsyncCacheProvider asyncCacheProvider)
        {
            _cacheKeys = new ConcurrentDictionary<string, object>();
            _asyncCacheProvider = asyncCacheProvider;
            _cachingStrategy = cachingStrategy;
            _retryPolicy = Policy
                .CacheAsync(_asyncCacheProvider,
                    new ContextualTtl());
        }

        /// <inheritdoc />
        public Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler)
        {
            var cachingTime = _cachingStrategy.GetCachingTime(targetMethod, args);
            
            if (cachingTime <= TimeSpan.Zero)
                return innerHandler();

            var contextData = new Dictionary<string, object>
            {
                {ContextualTtl.TimeSpanKey, cachingTime}
            };

            string cacheKey = GetExecutionKey(targetMethod, args);

            _readerWriterLock.EnterReadLock();
            try
            {
                _cacheKeys[cacheKey] = true;
                return _retryPolicy.ExecuteAsync((context, ct) => innerHandler(),
                    new Context(cacheKey, contextData), default);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public async Task InvalidateCache()
        {
            var cts = new CancellationTokenSource();

            _readerWriterLock.EnterWriteLock();
            try
            {
                var clearCacheKeys = _cacheKeys.Keys.ToArray();

                foreach (var cacheKey in clearCacheKeys)
                {
                    await _asyncCacheProvider.RemoveAsync(cacheKey, cts.Token);
                    _cacheKeys.TryRemove(cacheKey,out var x);
                }
            }
            finally 
            {
                _readerWriterLock.ExitWriteLock();
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