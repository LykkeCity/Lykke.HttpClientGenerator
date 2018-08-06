using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Caching
{
    public class ClientCacheManager : IClientCacheManager
    {
        public ClientCacheManager()
        {
        }

        public async Task InvalidateCacheAsync()
        {
            await (OnInvalidate?.Invoke() ?? Task.FromResult(0));
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
