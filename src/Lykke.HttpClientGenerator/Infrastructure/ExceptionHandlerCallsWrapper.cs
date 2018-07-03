using System;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Refit;

namespace Lykke.HttpClientGenerator.Infrastructure
{
    /// <summary>
    /// Calls wrapper to handle refit ApiException and throw ClientApiException (with HttpStatusCode and ErrorResponse) instead
    /// </summary>
    [PublicAPI]
    public class ExceptionHandlerCallsWrapper : ICallsWrapper
    {
        /// <inheritdoc />
        public async Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler)
        {
            try
            {
                return await innerHandler();
            }
            catch (ApiException ex)
            {
                var errResponse = ex.GetContentAs<ErrorResponse>();

                if (errResponse != null)
                    throw new ClientApiException(ex.StatusCode, errResponse);

                throw;
            }
        }
    }
}
