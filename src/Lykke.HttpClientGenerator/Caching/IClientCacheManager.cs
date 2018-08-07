using System;
using System.Collections.Generic;
using System.Text;
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
