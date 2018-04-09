using System;

namespace Lykke.ClientGenerator.Retries
{
    /// <summary>
    /// Describes the linear url request retry behavior
    /// </summary>
    public class LinearRetryStrategy : IRetryStrategy
    {
        public LinearRetryStrategy(TimeSpan retrySleepDuration, int retryAttemptsCount)
        {
            RetrySleepDuration = retrySleepDuration;
            RetryAttemptsCount = retryAttemptsCount;
        }

        /// <summary>
        /// Creates the strategy with default parameters: retry 6 times with sleeps of 5 second between tries.
        /// </summary>
        public LinearRetryStrategy() : this(TimeSpan.FromSeconds(5), 6)
        {
        }

        public int RetryAttemptsCount { get; }

        /// <summary>
        /// How much time to sleep between retries
        /// </summary>
        public TimeSpan RetrySleepDuration { get; }

        public TimeSpan GetRetrySleepDuration(int retryAttempt, string url) => RetrySleepDuration;
    }
}