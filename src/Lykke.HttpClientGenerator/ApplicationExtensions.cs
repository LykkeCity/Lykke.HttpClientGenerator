using System;
using Lykke.HttpClientGenerator.Exceptions;
using Microsoft.AspNetCore.Builder;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions to build the pipeline
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Uses refit exceptions handling middleware
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRefitExceptionHandler(this IApplicationBuilder app, Action<RefitExceptionHandlingOptions> configure = null)
        {
            var options = RefitExceptionHandlingOptions.CreateDefault();

            configure?.Invoke(options);

            app.UseMiddleware<RefitExceptionHandlerMiddleware>(options);

            return app;
        }
    }
}