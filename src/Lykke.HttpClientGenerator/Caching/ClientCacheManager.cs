using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Caching
{
    public class ClientCacheManager : IClientCacheManager
    {
        public ClientCacheManager()
        {
        }

        public Task InvalidateCacheAsync()
        {
            return OnInvalidate?.Invoke();
        }

        public event InvalidateCache OnInvalidate;

        public void Dispose()
        {
            if (OnInvalidate != null)
            {
                foreach (var d in OnInvalidate.GetInvocationList())
                {
                    if (d != null)
                    {
                        OnInvalidate -= (d as InvalidateCache);
                    }
                }
            }
        }
    }
}
