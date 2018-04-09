using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.ClientGenerator.Infrastructure
{
    public class UserAgentHeaderHttpClientHandler : DelegatingHandler
    {
        protected readonly string _userAgent;

        public UserAgentHeaderHttpClientHandler(HttpMessageHandler innerHandler, string userAgent)
            : base(innerHandler)
        {
            _userAgent = userAgent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            return base.SendAsync(request, cancellationToken);
        }
    }
}