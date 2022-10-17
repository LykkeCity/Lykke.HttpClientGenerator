using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lykke.HttpClientGenerator.Retries;

namespace Lykke.HttpClientGenerator.Tests
{
    public class TimeoutTests
    {
        [Test]
        public void RequestTimeout_IgnoresRetryPolicy()
        {
            // arrange
            var log = new List<string>();
            var fakeHttpClient = new FakeHttpClientHandler(r =>
            {
                Thread.Sleep(333);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("the result"))
                };
            });
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            // act && assert

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                var httpClient = HttpClientGenerator.BuildForUrl("http://google.com")
                    //Should be skipped because of timeout
                    .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromSeconds(1), 20))
                    .WithTimeout(timeout)
                    .Create()
                    .Generate<IRealInterface>();

                var result = await httpClient.GetHtmlAsync();
            });
        }

        [Test]
        public async Task Always_ExecutesWithoutTimeout()
        {
            // arrange
            var log = new List<string>();
            var fakeHttpClient = new FakeHttpClientHandler(r =>
            {
                Thread.Sleep(333);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("the result"))
                };
            });
            TimeSpan timeout = TimeSpan.FromMilliseconds(1000);

            // act && assert

                var httpClient = HttpClientGenerator.BuildForUrl("http://google.com")
                    .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromSeconds(1), 20))
                    .WithTimeout(timeout)
                    .Create()
                    .Generate<IRealInterface>();

                var result = await httpClient.GetHtmlAsync();
        }

        [Test]
        public async Task Always_WorksWithoutTimeout()
        {
            // arrange
            var log = new List<string>();
            var fakeHttpClient = new FakeHttpClientHandler(r =>
            {
                Thread.Sleep(333);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("the result"))
                };
            });
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            // act && assert

            var httpClient = HttpClientGenerator.BuildForUrl("http://google.com")
                //Should be skipped because of timeout
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromSeconds(1), 20))
                .Create()
                .Generate<IRealInterface>();

            var result = await httpClient.GetHtmlAsync();
        }
    }
}