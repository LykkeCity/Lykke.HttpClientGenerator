using System;
using System.Threading.Tasks;
using Lykke.HttpClientGenerator.Exceptions;
using Microsoft.Extensions.Logging;
using Refit;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Refit extensions
    /// </summary>
    public static class RefitExtensions
    {
        /// <summary>
        /// Execute action pased as a parameter and handle refit-specific exceptions, e. g. log them with details 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task WithExceptionDetailsAsync(Func<Task> action, ILogger logger, RefitExceptionHandlingOptions options = null)
        {
            options ??= RefitExceptionHandlingOptions.CreateDefault();
            
            try
            {
                await action();
            }
            catch (ValidationApiException e)
            {
                logger.LogError(e, e.GetDescription());
                if (options.ReThrow) throw;
            }
            catch (ApiException e)
            {
                logger.LogError(e, e.GetDescription());
                if (options.ReThrow) throw;
            }
            catch (HttpClientApiException e)
            {
                logger.LogError(e, e.GetDescription());
                if (options.ReThrow) throw;
            }
        }
    }
}