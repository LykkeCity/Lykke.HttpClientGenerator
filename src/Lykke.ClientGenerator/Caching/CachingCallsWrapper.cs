using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Lykke.ClientGenerator.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Caching.MemoryCache;
using Refit;

namespace Lykke.ClientGenerator.Caching
{
    public abstract class CachingCallsWrapper : ICallsWrapper
    {
        private readonly IAsyncPolicy _retryPolicy;

        protected CachingCallsWrapper()
        {
            _retryPolicy = Policy
                .CacheAsync(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                    new ContextualTtl());
        }

        public Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (targetMethod.GetCustomAttribute<GetAttribute>() == null)
                return innerHandler();
            
            var cachingTime = GetCachingTime(targetMethod, args);
            
            var contextData = new Dictionary<string, object>
            {
                {ContextualTtl.TimeSpanKey, cachingTime}
            };
            return _retryPolicy.ExecuteAsync((context, ct) => innerHandler(),
                new Context(GetExecutionKey(targetMethod, args), contextData), default);
        }

        protected abstract TimeSpan GetCachingTime(MethodInfo targetMethod, object[] args);

        private static string GetExecutionKey(MethodInfo targetMethod, object[] args)
        {
            return $"{targetMethod.DeclaringType}:{targetMethod.Name}:{targetMethod.GetHashCode()}:{JsonConvert.SerializeObject(args)}";
        }
    }
}