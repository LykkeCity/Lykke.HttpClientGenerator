using Common;
using Lykke.HttpClientGenerator.Exceptions;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Text extensions for <see cref="HttpClientApiException"/>
    /// </summary>
    public static class HttpClientApiExceptionTextExtensions
    {
        /// <summary>
        /// Get error response as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetErrorResponsePhrase(this HttpClientApiException exception)
        {
            if (exception.ErrorResponse == null)
                return string.Empty;

            return $"Error response: {exception.ErrorResponse.ToJson()}.";
        }

        /// <summary>
        /// Get status code as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetStatusCodePhrase(this HttpClientApiException exception)
        {
            return $"Status code: {exception.HttpStatusCode.ToString()}.";
        }
        
        /// <summary>
        /// Get exception description including all the details about error, as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetDescription(this HttpClientApiException exception)
        {
            return $"Couldn't execute http request. {GetErrorResponsePhrase(exception)} {GetStatusCodePhrase(exception)}";
        }
    }
}