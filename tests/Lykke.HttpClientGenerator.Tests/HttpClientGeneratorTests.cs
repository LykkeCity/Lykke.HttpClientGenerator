using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.HttpClientGenerator.Infrastructure;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Lykke.HttpClientGenerator.Tests
{
    public class HttpClientGeneratorTests
    {
        [Test]
        public async Task Always_ShouldCreateCreateHttpMessageHandlersCorrectly()
        {
            // arrange
            var log = new List<string>();
            var h1 = new TestDelegatingHandler(1, log);
            var h2 = new TestDelegatingHandler(2, log);
            var h3 = new FakeHttpClientHandler(r =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("the result"))
                });

            // act
            var result = await new HttpClientGenerator("http://fake.host", new ICallsWrapper[0],
                    new DelegatingHandler[] {h1, h2, h3})
                .Generate<ITestInterface>()
                .TestMethod();

            // assert
            result.Should().Be("the result");
            log.Should().BeEquivalentTo(new List<string>
            {
                "Handler 1 before",
                "Handler 2 before",
                "Handler 2 after",
                "Handler 1 after",
            }, o => o.WithStrictOrdering());
        }

        private class TestDelegatingHandler : DelegatingHandler
        {
            private readonly int _num;
            private readonly List<string> _log;

            public TestDelegatingHandler(int num, List<string> log)
            {
                _num = num;
                _log = log;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                _log.Add($"Handler {_num} before");
                var result = await base.SendAsync(request, cancellationToken);
                _log.Add($"Handler {_num} after");
                return result;
            }
        }
    }
}