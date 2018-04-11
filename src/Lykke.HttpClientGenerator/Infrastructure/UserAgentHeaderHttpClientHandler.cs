using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Infrastructure
{
    /// <summary>
    /// Adds User-Agent header to the request
    /// </summary>
    public class UserAgentHeaderHttpClientHandler : DelegatingHandler
    {
        protected readonly string _userAgent;

        /// <inheritdoc />
        public UserAgentHeaderHttpClientHandler(HttpMessageHandler innerHandler, string userAgent)
            : base(innerHandler)
        {
            _userAgent = userAgent;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            return base.SendAsync(request, cancellationToken);
        }
    }
}