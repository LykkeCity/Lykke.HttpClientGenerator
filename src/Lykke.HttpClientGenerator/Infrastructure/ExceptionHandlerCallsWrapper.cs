using System;
using System.Reflection;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Refit;

namespace Lykke.HttpClientGenerator.Infrastructure
{
    public class ExceptionHandlerCallsWrapper : ICallsWrapper
    {
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
