using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Tests
{
    public class FakeHttpClientHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _getResponse;

        public FakeHttpClientHandler(Func<HttpRequestMessage, HttpResponseMessage> getResponse)
        {
            _getResponse = getResponse;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCounter++;
            return Task.FromResult(_getResponse(request));
        }

        public int SendCounter { get; private set; }
    }
}