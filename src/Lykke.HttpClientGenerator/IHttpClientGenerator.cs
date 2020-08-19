using System;
using JetBrains.Annotations;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Generates client proxies for <see cref="Refit"/> interfaces
    /// </summary>
    [PublicAPI]
    public interface IHttpClientGenerator
    {
        /// <summary>
        /// Generates the proxy
        /// </summary>
        TInterface Generate<TInterface>();

        TInterface Generate<TInterface>(TimeSpan timeout);
    }
}