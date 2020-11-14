using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Refit;

namespace Lykke.HttpClientGenerator.Exceptions
{
    /// <summary>
    /// Refit specific exceptions handling middleware
    /// </summary>
    public class RefitExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RefitExceptionHandlingOptions _options;
        private readonly ILogger<RefitExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Creates middleware
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public RefitExceptionHandlerMiddleware(RequestDelegate next,
            ILogger<RefitExceptionHandlerMiddleware> logger,
            RefitExceptionHandlingOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// Executes upon middleware invocation
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationApiException e)
            {
                _logger.LogError(e, e.GetDescription());
                if (_options.ReThrow) throw;
            }
            catch (ApiException e)
            {
                _logger.LogError(e, e.GetDescription());
                if (_options.ReThrow) throw;
            }
            catch (HttpClientApiException e)
            {
                _logger.LogError(e, e.GetDescription());
                if (_options.ReThrow) throw;
            }
        }
    }
}