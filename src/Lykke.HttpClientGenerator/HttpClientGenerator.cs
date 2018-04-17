using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Lykke.HttpClientGenerator.Infrastructure;
using Refit;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Generates client proxies for <see cref="Refit"/> interfaces
    /// </summary>
    /// <remarks>
    /// By default adds custom headers, caching and retries.
    /// To disable caching provide empty callsWrappers.
    /// To disable retries provide null for the retryStrategy.
    /// </remarks>
    public class HttpClientGenerator : IHttpClientGenerator
    {
        private readonly string _rootUrl;
        private readonly RefitSettings _refitSettings;
        private readonly List<ICallsWrapper> _wrappers;

        /// <summary>
        /// Kicks-off the fluent interface of building a configured <see cref="HttpClientGenerator"/>
        /// </summary>
        public static HttpClientGeneratorBuilder BuildForUrl(string rootUrl)
        {
            return new HttpClientGeneratorBuilder(rootUrl);
        }

        /// <inheritdoc />
        public HttpClientGenerator(string rootUrl, IEnumerable<ICallsWrapper> callsWrappers,
            IEnumerable<DelegatingHandler> httpDelegatingHandlers)
        {
            _rootUrl = rootUrl;
            var httpMessageHandler = CreateHttpMessageHandler(httpDelegatingHandlers.ToList().GetEnumerator());
            _refitSettings = new RefitSettings {HttpMessageHandlerFactory = () => httpMessageHandler};
            _wrappers = callsWrappers.ToList();
        }

        /// <summary>
        /// Generates the proxy
        /// </summary>
        public TInterface Generate<TInterface>()
        {
            return WrapIfNeeded(RestService.For<TInterface>(_rootUrl, _refitSettings));
        }

        /// <summary>
        /// Constructs <see cref="HttpMessageHandler"/> from an enumerable of delegating handlers 
        /// </summary>
        private static HttpMessageHandler CreateHttpMessageHandler(IEnumerator<DelegatingHandler> handlersEnumerator)
        {
            if (handlersEnumerator.MoveNext())
            {
                return handlersEnumerator.Current.InnerHandler = CreateHttpMessageHandler(handlersEnumerator);
            }
            else
            {
                // if no more handlers found - add the handler actually making the calls
                return new HttpClientHandler();
            }
        }

        private T WrapIfNeeded<T>(T obj)
        {
            return _wrappers.Count > 0
                ? AopProxy.Create(obj, _wrappers.Select(w => (AopProxy.MethodCallHandler) w.HandleMethodCall).ToArray())
                : obj;
        }
    }
}