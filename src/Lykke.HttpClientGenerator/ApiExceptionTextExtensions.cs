using Common;
using Refit;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Text extensions for <see cref="ApiException"/>
    /// </summary>
    public static class ApiExceptionTextExtensions
    {
        /// <summary>
        /// Get http request caused the exception as text, except for the headers
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetRequestPhrase(this ApiException exception)
        {
            if (exception.RequestMessage == null)
            {
                return string.Empty;
            }
            
            var requestDataToText = new
            {
                exception.RequestMessage.Version,
                exception.RequestMessage.Content,
                exception.RequestMessage.RequestUri,
                exception.RequestMessage.Method
            };
            
            return $"Request: {requestDataToText.ToJson()}.";   
        }

        /// <summary>
        /// Get response content as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetContentPhrase(this ApiException exception)
        {
            return exception.Content != null ? $"Content: {exception.Content.ToJson()}." : string.Empty;
        }

        /// <summary>
        /// Get exception data as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetDataPhrase(this ApiException exception)
        {
            if (exception.Data.Count == 0)
                return string.Empty;

            return $"Data: {exception.Data.ToJson()}.";
        }

        /// <summary>
        /// Get exception description including all the details about request and error, as text
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetDescription(this ApiException exception)
        {
            return $"Couldn't execute http request. Reason: {exception.ReasonPhrase}. {exception.GetContentPhrase()} {exception.GetDataPhrase()} {exception.GetRequestPhrase()}";
        }
    }
}