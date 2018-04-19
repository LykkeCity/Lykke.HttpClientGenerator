using System.Threading.Tasks;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    public interface ITestInterface
    {
        [Get("/fake/url/")]
        Task<string> TestMethod();
    }
}