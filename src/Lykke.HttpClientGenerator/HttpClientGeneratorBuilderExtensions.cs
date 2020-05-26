using System;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Extension methods for <see cref="HttpClientGeneratorBuilder"/>
    /// </summary>
    public static class HttpClientGeneratorBuilderExtensions
    {
        /// <summary>
        /// Sets a name for this service which will be displayed in all errors
        /// </summary>
        ///
        /// <param name="builder">The builder for the service</param>
        /// <param name="serviceName">The name of the service</param>
        ///
        /// <returns>The service builder</returns>
        ///
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="builder"/> or <paramref name="serviceName"/> are null
        /// </exception>
        public static HttpClientGeneratorBuilder WithServiceName(
            this HttpClientGeneratorBuilder builder,
            string serviceName)
        {
            return AttachCallsWrapper<object>(builder, serviceName, showReason: false);
        }

        /// <summary>
        /// Sets a name for this service which will be displayed in all errors,
        /// additionally parsing and adding service errors
        /// </summary>
        ///
        /// <param name="builder">The builder for the service</param>
        /// <param name="serviceName">The name of the service</param>
        ///
        /// <typeparam name="T">
        /// The type representing the structure
        /// of the errors returned by the service
        /// </typeparam>
        ///
        /// <returns>The service builder</returns>
        ///
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="builder"/> or <paramref name="serviceName"/> are null
        /// </exception>
        public static HttpClientGeneratorBuilder WithServiceName<T>(
            this HttpClientGeneratorBuilder builder,
            string serviceName)
            where T : class
        {
            return AttachCallsWrapper<T>(builder, serviceName, showReason: true);
        }

        private static HttpClientGeneratorBuilder AttachCallsWrapper<TApiError>(
            HttpClientGeneratorBuilder builder,
            string serviceName,
            bool showReason)
            where TApiError : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            var callsWrapper = new ServiceNameCallsWrapper<TApiError>(serviceName, showReason);
            return builder.WithAdditionalCallsWrapper(callsWrapper);
        }
    }
}