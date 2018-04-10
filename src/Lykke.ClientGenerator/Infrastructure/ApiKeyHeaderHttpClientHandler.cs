﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.ClientGenerator.Infrastructure
{
    /// <summary>
    /// Adds api-key header to the request
    /// </summary>
    public class ApiKeyHeaderHttpClientHandler : DelegatingHandler
    {
        protected readonly string _apiKey;

        /// <inheritdoc />
        public ApiKeyHeaderHttpClientHandler(HttpMessageHandler innerHandler, string apiKey)
            : base(innerHandler)
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