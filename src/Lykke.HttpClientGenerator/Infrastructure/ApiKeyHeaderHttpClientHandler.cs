using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Infrastructure
{
    /// <summary>
    /// Adds api-key header to the request
    /// </summary>
    public class ApiKeyHeaderHttpClientHandler : DelegatingHandler
    {
        private readonly string _apiKey;

        /// <inheritdoc />
        public ApiKeyHeaderHttpClientHandler(string apiKey)
        {
            _apiKey = apiKey;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("api-key", _apiKey);
            return base.SendAsync(request, cancellationToken);
        }
    }
}