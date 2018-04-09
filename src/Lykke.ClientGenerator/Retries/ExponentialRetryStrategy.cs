using System;

namespace Lykke.ClientGenerator.Retries
{
    /// <summary>
    /// Describes the exponential url request retry behavior
    /// </summary>
    public class ExponentialRetryStrategy : IRetryStrategy
    {
        public ExponentialRetryStrategy(TimeSpan maxRetrySleepDuration, int retryAttemptsCount, double exponentBase)
        {
            _maxRetrySleepDuration = maxRetrySleepDuration;
            RetryAttemptsCount = retryAttemptsCount;
            _exponentBase = exponentBase;
        }

        /// <summary>
        /// Creates the strategy with default parameters: retry max 6 times
        /// starting with 2 seconds and multiplying it by 2 evety time 
        /// and with max sleeps of 1 minute.
        /// </summary>
        public ExponentialRetryStrategy() : this(TimeSpan.FromMinutes(1), 6, 2)
        {
        }

        /// <summary>
        /// Maximum time to sleep between retries
        /// </summary>
        private readonly TimeSpan _maxRetrySleepDuration;

        private readonly double _exponentBase;

        public int RetryAttemptsCount { get; }
        
        public TimeSpan GetRetrySleepDuration(int retryAttempt, string url)
        {
            return TimeSpan.FromSeconds(Math.Min(_maxRetrySleepDuration.TotalSeconds, Math.Pow(_exponentBase, retryAttempt)));
        }
    }
}