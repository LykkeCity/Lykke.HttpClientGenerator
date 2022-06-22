using System.Net.Http;

namespace Lykke.HttpClientGenerator
{
    public static class HttpRequestMessageExtensions
    {
        public static object ToLogModel(this HttpRequestMessage source) =>
            new
            {
                source.RequestUri,
                source.Method,
                source.Version,
                source.Content
            };
    }
}