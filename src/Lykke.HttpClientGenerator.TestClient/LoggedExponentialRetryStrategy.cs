using Lykke.HttpClientGenerator.Retries;

namespace Lykke.HttpClientGenerator.TestClient
{
    public class LoggedExponentialRetryStrategy : ExponentialRetryStrategy
    {
        public override TimeSpan GetRetrySleepDuration(int retryAttempt, string url)
        {
            Console.WriteLine($"Attempt {retryAttempt}, url {url}");
            return base.GetRetrySleepDuration(retryAttempt, url);
        }
    }
}