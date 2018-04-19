using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Lykke.HttpClientGenerator.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Caching.MemoryCache;

namespace Lykke.HttpClientGenerator.Caching
{
    /// <summary>
    /// Adds caching to method calls.
    /// </summary>
    public class CachingCallsWrapper : ICallsWrapper
    {
        private readonly ICachingStrategy _cachingStrategy;
        private readonly IAsyncPolicy _retryPolicy;

        /// <inheritdoc />
        public CachingCallsWrapper(ICachingStrategy cachingStrategy)
        {
            _cachingStrategy = cachingStrategy;
            _retryPolicy = Policy
                .CacheAsync(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
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
            return _retryPolicy.ExecuteAsync((context, ct) => innerHandler(),
                new Context(GetExecutionKey(targetMethod, args), contextData), default);
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