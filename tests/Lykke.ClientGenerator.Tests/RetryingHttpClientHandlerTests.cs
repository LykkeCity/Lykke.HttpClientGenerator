using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.ClientGenerator.Retries;
using NUnit.Framework;
using Refit;

namespace Lykke.ClientGenerator.Tests
{
    public class RetryingHttpClientHandlerTests
    {
        [Test]
        public void Always_ShouldRetryCorrectly()
        {
            // arrange
            var fakeHttpClientHandler = new FakeHttpClientHandler();
            var refitSettings = new RefitSettings
            {
                HttpMessageHandlerFactory = () => new RetryingHttpClientHandler(fakeHttpClientHandler,
                    new LinearRetryStrategy(TimeSpan.FromMilliseconds(1), 6))
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

    public interface ITestInterface
    {
        [Get("/fake/url/")]
        Task<string> TestMethod();
    }

    public class FakeHttpClientHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCounter++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway));
        }
        
        public int SendCounter { get; private set; }
    }
}