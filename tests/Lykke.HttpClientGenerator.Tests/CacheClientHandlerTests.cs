using System;
using System.Collections;
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
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.HttpClientGenerator.Retries;
using NUnit.Framework;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    public class CacheClientHandlerTests
    {
        [Test]
        public async Task WorksCorrectlyForOneThread_InvalidateCacheAsync()
        {
            string result1Str = "the result 1";
            string result2Str = "the result 2";
            int counter = 0;
            // arrange
            var h3 = new FakeHttpClientHandler(r =>
            {
                counter++;

                if (counter > 1)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(Encoding.UTF8.GetBytes(result2Str))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(result1Str))
                };
            });

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
            result.Should().Be(result1Str);
            result1.Should().Be(result1Str);
            result2.Should().Be(result2Str);
        }

        [Test]
        public async Task WorksCorrectlyForManyThreads_InvalidateCacheAsync()
        {
            string result1Str = "the result 1";
            string result2Str = "the result 2";
            int counter = 0;
            // arrange
            var h3 = new FakeHttpClientHandler(r =>
            {
                counter++;

                string response = counter % 2 == 0 ? result2Str : result1Str;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(response))
                };
            });

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
            var res2Count = results.Count(x => x.Contains( result2Str));
            // assert
            res2Count.Should().BeGreaterThan(0);
        }
    }
}