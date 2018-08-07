using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Utilities;

namespace Lykke.HttpClientGenerator.Caching
{
    public class RemovableAsyncCacheProvider : IRemovableAsyncCacheProvider
    {
        private readonly IMemoryCache _cache;

        public RemovableAsyncCacheProvider(IMemoryCache memoryCache)
        {
            if (memoryCache == null) throw new ArgumentNullException(nameof(memoryCache));
            _cache = memoryCache;
        }

        public object Get(String key)
        {
            object value;
            if (_cache.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public void Put(string key, object value, Ttl ttl)
        {
            TimeSpan remaining = DateTimeOffset.MaxValue - SystemClock.DateTimeOffsetUtcNow();
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

            if (ttl.SlidingExpiration)
            {
                options.SlidingExpiration = ttl.Timespan < remaining ? ttl.Timespan : remaining;
            }
            else
            {
                if (ttl.Timespan == TimeSpan.MaxValue)
                {
                    options.AbsoluteExpiration = DateTimeOffset.MaxValue;
                }
                else
                {
                    options.AbsoluteExpirationRelativeToNow = ttl.Timespan < remaining ? ttl.Timespan : remaining;
                }
            }

            _cache.Set(key, value, options);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public Task<object> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return TaskHelper.FromResult(Get(key));
        }

        public Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Put(key, value, ttl);

            return TaskHelper.EmptyTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Remove(key);

            return TaskHelper.EmptyTask;
        }
    }
}
