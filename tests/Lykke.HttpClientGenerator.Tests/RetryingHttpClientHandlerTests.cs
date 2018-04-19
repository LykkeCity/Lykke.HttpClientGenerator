using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Lykke.HttpClientGenerator.Retries;
using NUnit.Framework;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    public class RetryingHttpClientHandlerTests
    {
        [Test]
        public void Always_ShouldRetryCorrectly()
        {
            // arrange
            var fakeHttpClientHandler = new FakeHttpClientHandler(r => new HttpResponseMessage(HttpStatusCode.BadGateway));
            var refitSettings = new RefitSettings
            {
                HttpMessageHandlerFactory = () =>
                    new RetryingHttpClientHandler(new LinearRetryStrategy(TimeSpan.FromMilliseconds(1), 6))
                    {
                        InnerHandler = fakeHttpClientHandler
                    }
            };

            var proxy = RestService.For<ITestInterface>("http://fake.host", refitSettings);

            // act
            var invocation = proxy.Invoking(p => p.TestMethod().GetAwaiter().GetResult());

            // assert
            invocation.Should().Throw<ApiException>()
                .WithMessage("Response status code does not indicate success: 502 (Bad Gateway).");
            fakeHttpClientHandler.SendCounter.Should().Be(7);
        }
    }
}