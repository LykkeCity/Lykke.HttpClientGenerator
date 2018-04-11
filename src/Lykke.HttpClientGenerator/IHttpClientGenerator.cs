namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Generates client proxies for <see cref="Refit"/> interfaces
    /// </summary>
    public interface IHttpClientGenerator
    {
        /// <summary>
        /// Generates the proxy
        /// </summary>
        TInterface Generate<TInterface>();
    }
}