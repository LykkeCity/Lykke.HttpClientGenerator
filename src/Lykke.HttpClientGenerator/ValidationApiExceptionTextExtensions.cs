using Common;
using Lykke.HttpClientGenerator.Retries;
using Refit;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Text extensions for <see cref="ValidationApiException"/>
    /// </summary>
    public static class ValidationApiExceptionTextExtensions
    {
        /// <summary>
        /// Get http request caused the exception problem details as text 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetProblemDetailsPhrase(this ValidationApiException exception)
        {
            return exception.Content != null ? $"Problem details: {exception.Content.ToJson()}." : string.Empty;
        }

        /// <summary>
        /// Get exception description including all the details about request and error, as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetDescription(this ValidationApiException exception)
        {
            return $"Couldn't execute http request. {exception.GetProblemDetailsPhrase()} {exception.GetRequestPhrase()}";
        }
    }
}