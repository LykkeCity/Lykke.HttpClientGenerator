using System.Threading.Tasks;
using Lykke.HttpClientGenerator.Caching;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    public interface ITestInterface
    {
        [Get("/fake/url/")]
        Task<string> TestMethod();

        [Get("/fake/url/")]
        [ClientCaching(Minutes = 29)]
        Task<string> TestMethodWithCache();
    }

    public interface IRealInterface
    {
        [Get("/")]
        Task<string> GetHtmlAsync();
    }
}