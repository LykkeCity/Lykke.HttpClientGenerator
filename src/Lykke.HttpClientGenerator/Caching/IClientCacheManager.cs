using System;
using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Caching
{
    public delegate Task InvalidateCache();

    public interface IClientCacheManager : IDisposable
    {
        Task InvalidateCacheAsync();

        event InvalidateCache OnInvalidate;
    }
}
