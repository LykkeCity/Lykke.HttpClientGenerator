using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Polly;
using Polly.Retry;

namespace Lykke.ClientGenerator.Retries
{
    /// <summary>
    /// Adds retries to the http request
    /// </summary>
    public class RetryingHttpClientHandler : DelegatingHandler
    {
        protected readonly RetryPolicy _retryPolicy;

        /// <inheritdoc />
        public RetryingHttpClientHandler([NotNull] HttpMessageHandler innerHandler, IRetryStrategy retryStrategy)
            : base(innerHandler)
        {
            if (innerHandler == null) throw new ArgumentNullException(nameof(innerHandler));
            if (retryStrategy == null) throw new ArgumentNullException(nameof(retryStrategy));

            var retryAttemptsCount = retryStrategy.RetryAttemptsCount;
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(retryAttemptsCount, 
                    (retryAttempt, context) => retryStrategy.GetRetrySleepDuration(retryAttempt, context.ExecutionKey),
                    (exception, timeSpan, retryAttempt, context) => context["RetriesLeft"] = retryAttemptsCount - retryAttempt);
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _retryPolicy.ExecuteAsync(async (context, ct) =>
            {
                var response = await base.SendAsync(request, ct);
                if ((!context.TryGetValue("RetriesLeft", out var retriesLeft) || (int) retriesLeft > 0) &&
                    !response.IsSuccessStatusCode &&
                    response.StatusCode != HttpStatusCode.BadRequest &&
                    response.StatusCode != HttpStatusCode.InternalServerError)
                {
                    // throws to execute retry
                    response.EnsureSuccessStatusCode();
                }

                return response;
            }, new Context(request.RequestUri.ToString()), cancellationToken);
        }
    }
}