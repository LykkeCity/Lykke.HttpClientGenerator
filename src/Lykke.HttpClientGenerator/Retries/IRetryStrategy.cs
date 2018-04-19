using System;

namespace Lykke.HttpClientGenerator.Retries
{
    /// <summary>
    /// Describes the url request retry behavior
    /// </summary>
    public interface IRetryStrategy
    {
        /// <summary>
        /// Generates retry sleep duration from retry attempt number and request url
        /// </summary>
        /// <param name="retryAttempt">Current retry number, from 1</param>
        /// <param name="url">Url of request being retried</param>
        TimeSpan GetRetrySleepDuration(int retryAttempt, string url);

        /// <summary>
        /// How many times to retry in case of error.
        /// </summary>
        /// <remarks>
        /// If n + 1 request was made without success - execution ends meaning failure.
        /// </remarks>
        int RetryAttemptsCount { get; }
    }
}