using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Lykke.ClientGenerator.Caching;
using Lykke.ClientGenerator.Infrastructure;
using Lykke.ClientGenerator.Retries;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.ClientGenerator
{
    /// <summary>
    /// Generates client proxies for <see cref="Refit"/> interfaces
    /// </summary>
    /// <remarks>
    /// By default adds custom headers, caching and retries.
    /// To disable caching provide empty callsWrappers.
    /// To disable retries provide null for the retryStrategy.
    /// </remarks>
    public class ClientProxyGenerator : IClientProxyGenerator
    {
        private readonly string _rootUrl;
        private readonly RefitSettings _refitSettings;
        private readonly List<ICallsWrapper> _wrappers;

        /// <summary>
        /// Creates the generator without proxy key and with default retry and caching settings
        /// </summary>
        public static ClientProxyGenerator CreateDefault(string rootUrl)
        {
            return CreateDefault(rootUrl, null);
        }

        /// <summary>
        /// Creates the generator with proxy key and with default retry and caching settings
        /// </summary>
        public static ClientProxyGenerator CreateDefault(string rootUrl, [CanBeNull] string apiKey)
        {
            return CreateDefault(rootUrl, apiKey, new LinearRetryStrategy());
        }

        /// <summary>
        /// Creates the generator with proxy key, default caching settings
        /// and with specified <paramref name="retryStrategy"/>. If it is null - no retries will be executed.
        /// </summary>
        public static ClientProxyGenerator CreateDefault(string rootUrl, [CanBeNull] string apiKey,
            [CanBeNull] IRetryStrategy retryStrategy)
        {
            return new ClientProxyGenerator(rootUrl, GetDefaultCallsWrappers(),
                GetDefaultHttpMessageHandlerProviders(retryStrategy, apiKey));
        }

        /// <summary>
        /// Creates customized generator 
        /// </summary>
        public static ClientProxyGenerator CreateCustom(string rootUrl, IEnumerable<ICallsWrapper> callsWrappers,
            IEnumerable<Func<HttpMessageHandler, HttpMessageHandler>> httpMessageHandlerProviders)
        {
            return new ClientProxyGenerator(rootUrl, callsWrappers, httpMessageHandlerProviders);
        }

        private ClientProxyGenerator(string rootUrl, IEnumerable<ICallsWrapper> callsWrappers,
            IEnumerable<Func<HttpMessageHandler, HttpMessageHandler>> httpMessageHandlerProviders)
        {
            _rootUrl = rootUrl;
            var httpMessageHandler = CreateHttpMessageHandler(httpMessageHandlerProviders.ToList().GetEnumerator());
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
        /// Gets default proxy calls wrappers: the one to cache calls results based on the
        /// <see cref="ClientCachingAttribute"/>
        /// </summary>
        public static IEnumerable<ICallsWrapper> GetDefaultCallsWrappers()
        {
            yield return new AttributeBasedCachingCallsWrapper();
        }

        /// <summary>
        /// Gets default http handler providers: add api-key header, add User-Agent header, and apply retry strategy
        /// </summary>
        public static IEnumerable<Func<HttpMessageHandler, HttpMessageHandler>> GetDefaultHttpMessageHandlerProviders(
            IRetryStrategy retryStrategy = null, string apiKey = null)
        {
            if (apiKey != null)
            {
                yield return h => new ApiKeyHeaderHttpClientHandler(h, apiKey);
            }

            yield return h => new UserAgentHeaderHttpClientHandler(h, GetUserAgent());

            if (retryStrategy != null)
            {
                yield return h => new RetryingHttpClientHandler(h, retryStrategy);
            }
        }

        /// <summary>
        /// Constructs <see cref="HttpMessageHandler"/> from an array of wrapping handler providers 
        /// </summary>
        private static HttpMessageHandler CreateHttpMessageHandler(
            IEnumerator<Func<HttpMessageHandler, HttpMessageHandler>> handlerProvidersEnumerator)
        {
            if (handlerProvidersEnumerator.MoveNext())
            {
                return handlerProvidersEnumerator.Current.Invoke(CreateHttpMessageHandler(handlerProvidersEnumerator));
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
                ? AopProxy.Create(obj,
                    _wrappers.Select(w => (AopProxy.MethodCallHandler) w.HandleMethodCall).ToArray())
                : obj;
        }

        private static string GetUserAgent()
        {
            return
                $"{PlatformServices.Default.Application.ApplicationVersion} v{PlatformServices.Default.Application.ApplicationVersion}";
        }
    }
}