using Lykke.HttpClientGenerator.Retries;

namespace Lykke.HttpClientGenerator.TestClient
{
    public class LoggedExponentialRetryStrategy : IRetryStrategy
    {
        private readonly ExponentialRetryStrategy _strategy;

        public LoggedExponentialRetryStrategy(ExponentialRetryStrategy strategy)
        {
            _strategy = strategy;
        }

        public TimeSpan GetRetrySleepDuration(int retryAttempt, string url)
        {
            Console.WriteLine($"Attempt {retryAttempt}, url {url}");
            return _strategy.GetRetrySleepDuration(retryAttempt, url);
        }

        public int RetryAttemptsCount => _strategy.RetryAttemptsCount;
    }
}