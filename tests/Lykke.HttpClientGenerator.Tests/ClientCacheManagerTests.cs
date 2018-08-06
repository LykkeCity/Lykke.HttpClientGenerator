using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.HttpClientGenerator.Caching;

namespace Lykke.HttpClientGenerator.Tests
{
    public class ClientCacheManagerTests
    {
        [Test]
        public async Task WhenObjectDisposed_UnsubscribeAll()
        {
            var subscribers = Enumerable.Range(0, 10)
                .Select(x => new MockSubscriber())
                .ToList();

            ClientCacheManager cacheManager = new ClientCacheManager();

            foreach (var subscriber in subscribers)
            {
                cacheManager.OnInvalidate += subscriber.InvalidateCacheAsync;
            }

            await cacheManager.InvalidateCacheAsync();
            cacheManager.Dispose();
            await cacheManager.InvalidateCacheAsync();

            foreach (var subscriber in subscribers)
            {
                //1 Time for each subscriber
                subscriber.CacheInvalidatedCallsCount.Should().Be(1);
            }
        }

        [Test]
        public async Task WhenObjectNotDisposed_InvalidateAllOnEachCall()
        {
            var subscribers = Enumerable.Range(0, 10)
                .Select(x => new MockSubscriber())
                .ToList();

            ClientCacheManager cacheManager = new ClientCacheManager();

            foreach (var subscriber in subscribers)
            {
                cacheManager.OnInvalidate += subscriber.InvalidateCacheAsync;
            }

            await cacheManager.InvalidateCacheAsync();
            await cacheManager.InvalidateCacheAsync();

            foreach (var subscriber in subscribers)
            {
                //2 Time for each subscriber
                subscriber.CacheInvalidatedCallsCount.Should().Be(2);
            }
        }
    }

    internal class MockSubscriber
    {
        public int CacheInvalidatedCallsCount { get; private set; }

        public Task InvalidateCacheAsync()
        {
            CacheInvalidatedCallsCount++;

            return Task.CompletedTask;
        }
    }
}