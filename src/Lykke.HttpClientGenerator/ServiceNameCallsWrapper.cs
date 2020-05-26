using System;
using System.Reflection;
using System.Threading.Tasks;
using Lykke.HttpClientGenerator.Infrastructure;
using Newtonsoft.Json;
using Refit;

namespace Lykke.HttpClientGenerator
{
    internal class ServiceNameCallsWrapper<T> : ICallsWrapper
        where T : class
    {
        private readonly string _serviceName;
        private readonly bool _showReason;

        public ServiceNameCallsWrapper(string serviceName, bool showReason)
        {
            _serviceName = serviceName;
            _showReason = showReason;
        }

        public async Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler)
        {
            try
            {
                return await innerHandler();
            }
            catch (Exception e)
            {
                var httpPathAttr = targetMethod.GetCustomAttribute<HttpMethodAttribute>();
                var apiErrorDetails = string.Empty;
                if (_showReason && e is ApiException apiException)
                {
                    var reason = string.IsNullOrWhiteSpace(apiException.Content)
                        ? "<unknown>"
                        : $"{JsonConvert.DeserializeObject<T>(apiException.Content)}";
                    apiErrorDetails = $" with reason '{reason}'";
                }

                var endpoint = httpPathAttr?.Path ?? $"{targetMethod.DeclaringType.Name}.{targetMethod.Name}";
                var message = $"An error occurred while trying to reach {_serviceName} ({endpoint}){apiErrorDetails}: {e.Message}";

                // Have to use reflection here, otherwise can't change message without wrapping exception
                // which would conflict with the requirement of not losing exception type
                //
                // This field will be present on all exceptions, as it is defined on the base Exception type
                e.SetMessage(message);

                // Rethrow the now modified exception to preserve the stack trace
                throw;
            }
        }
    }
}