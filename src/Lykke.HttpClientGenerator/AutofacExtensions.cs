using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator.Infrastructure;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Autofac extension to register refit clients.
    /// </summary>
    [PublicAPI]
    public static class AutofacExtensions
    {
        /// <summary>
        /// Registers Refit client of type <typeparamref name="TInterface"/>.
        /// </summary>
        public static void RegisterClient<TInterface>(
            [NotNull] this ContainerBuilder builder,
            [NotNull] string serviceUrl,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure = null)
            where TInterface : class
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be empty.", nameof(serviceUrl));

            var clientBuilder = HttpClientGenerator.BuildForUrl(serviceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());
            
            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder
                .RegisterInstance(clientBuilder.Create().Generate<TInterface>())
                .As<TInterface>()
                .SingleInstance();
        }
    }
}
