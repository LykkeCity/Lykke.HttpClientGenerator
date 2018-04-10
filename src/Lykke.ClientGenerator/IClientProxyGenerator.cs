namespace Lykke.ClientGenerator
{
    /// <summary>
    /// Generates client proxies for <see cref="Refit"/> interfaces
    /// </summary>
    public interface IClientProxyGenerator
    {
        /// <summary>
        /// Generates the proxy
        /// </summary>
        TInterface Generate<TInterface>();
    }
}