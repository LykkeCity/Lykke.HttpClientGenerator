using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.HttpClientGenerator.Caching;
using NUnit.Framework;

namespace Lykke.HttpClientGenerator.Tests
{
    public class CacheClientHandlerTests
    {
        private static string _result1Str = "the result 1";
        private static string _result2Str = "the result 2";

        [Test]
        public async Task WorksCorrectlyForOneThread_WithNoCachingManager()
        {
            // arrange
            var (h3, y1) = MakeFakeHttpClientHandler();

            // act
            var httpBuilder = HttpClientGenerator.BuildForUrl("http://fake.host");
            var testInterface = httpBuilder
                .WithCachingStrategy(new AttributeBasedCachingStrategy())
                .WithAdditionalDelegatingHandler(h3)
                .Create()
                .Generate<ITestInterface>();

            var result = await testInterface.TestMethodWithCache();
            var result2 = await testInterface.TestMethod();
            // assert
            result.Should().Be(_result1Str);
            result2.Should().Be(_result2Str);
        }

        [Test]
        public async Task WorksCorrectlyForOneThread_InvalidateCacheAsync()
        {
            // arrange
            var (h3, y1) = MakeFakeHttpClientHandler();

            // act

            var httpBuilder = HttpClientGenerator.BuildForUrl("http://fake.host");
            var cacheManager = new ClientCacheManager();
            var testInterface = httpBuilder
                .WithCachingStrategy(new AttributeBasedCachingStrategy())
                .WithAdditionalDelegatingHandler(h3)
                .Create(cacheManager)
                .Generate<ITestInterface>();

            var result = await testInterface.TestMethodWithCache();
            var result1 = await testInterface.TestMethodWithCache();
            cacheManager.InvalidateCacheAsync().Wait();
            var result2 = await testInterface.TestMethodWithCache();
            cacheManager.Dispose();
            // assert
            result.Should().Be(_result1Str);
            result1.Should().Be(_result1Str);
            result2.Should().Be(_result2Str);
        }

        [Test]
        public async Task WorksCorrectlyForManyThreads_InvalidateCacheAsync()
        {
            // arrange
            var (h3, y1) = MakeFakeHttpClientHandler();

            var bag = new ConcurrentBag<List<string>>();
            var httpBuilder = HttpClientGenerator.BuildForUrl("http://fake.host");
            var cacheManager = new ClientCacheManager();
            var testInterface = httpBuilder
                .WithCachingStrategy(new AttributeBasedCachingStrategy())
                .WithAdditionalDelegatingHandler(h3)
                .Create(cacheManager)
                .Generate<ITestInterface>();

            var cacheModifyingTasks = Enumerable
                .Range(0, 100)
                .Select(x => Task.Factory.StartNew(async () =>
                {
                    var list = new List<string>(2);
                    var result1 = await testInterface.TestMethodWithCache();
                    await cacheManager.InvalidateCacheAsync();
                    var result2 = await testInterface.TestMethodWithCache();

                    list.Add($"{Thread.CurrentThread.Name} {result1}");
                    list.Add($"{Thread.CurrentThread.Name} {result2}");
                    bag.Add(list);
                }).Unwrap());

            // act

            await Task.WhenAll(cacheModifyingTasks);
            cacheManager.Dispose();
            var results = bag.ToArray().SelectMany(x => x);
            var res2Count = results.Count(x => x.Contains(_result2Str));
            // assert
            res2Count.Should().BeGreaterThan(0);
        }

        private static (FakeHttpClientHandler, MockCounter) MakeFakeHttpClientHandler()
        {
            MockCounter mockCounter = new MockCounter();

            return (new FakeHttpClientHandler(r =>
            {
                var counter =  ++mockCounter.Counter;

                string response = counter % 2 == 0 ? _result2Str : _result1Str;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(response))
                };
            }), mockCounter);
        }

        private class MockCounter
        {
            public int Counter { get; set; }

            public MockCounter()
            {
            }
        }
    }
}