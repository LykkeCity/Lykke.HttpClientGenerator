using System;
using System.Net;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;

namespace Lykke.HttpClientGenerator.Exceptions
{
    /// <summary>
    /// client api exception
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class HttpClientApiException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public ErrorResponse ErrorResponse { get; set; }
        
        public HttpClientApiException(HttpStatusCode statusCode, ErrorResponse errorResponse):base(errorResponse.ErrorMessage)
        {
            HttpStatusCode = statusCode;
            ErrorResponse = errorResponse;
        }
    }
}