using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    public class LogRequestErrorHttpClientHandlerTests
    {
        [Test]
        public async Task ShouldLogRequestResponse()
        {
            // Arrange

            var fakeHttpClientHandler = new FakeHttpClientHandler(r =>
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("testError")
                }
            );
            var logFactory = new Mock<ILogFactory>();
            var log = new Mock<ILog>();

            logFactory
                .Setup(x => x.CreateLog(It.IsAny<object>()))
                .Returns(log.Object);

            var client = HttpClientGenerator.BuildForUrl("http://fake.host")
                .WithoutCaching()
                .WithoutRetries()
                .WithRequestErrorLogging(logFactory.Object)
                .WithAdditionalDelegatingHandler(fakeHttpClientHandler)
                .Create()
                .Generate<ITestInterface>();

            // Act

            try
            {
                await client.TestMethod();
            }
            catch(ApiException ex) when(ex.StatusCode == HttpStatusCode.BadRequest)
            {
            }

            // Arrange

            log.Verify(x =>
                x.Log<LogEntryParameters>(
                    It.Is<LogLevel>(v => v == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<LogEntryParameters>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<LogEntryParameters, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}