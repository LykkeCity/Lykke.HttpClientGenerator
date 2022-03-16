using System;
using System.Collections.Generic;
using System.Net.Http;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.HttpClientGenerator.Caching;
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.HttpClientGenerator.Retries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Refit;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Provides a simple interface for configuring the <see cref="HttpClientGenerator"/> for friquient use-cases
    /// Warning! By default the Caching Strategy  is AttributeBasedCachingStrategy.
    /// </summary>
    [PublicAPI]
    public class HttpClientGeneratorBuilder
    {
        /// <inheritdoc />
        public HttpClientGeneratorBuilder([NotNull] string rootUrl)
        {
            _rootUrl = rootUrl ?? throw new ArgumentNullException(nameof(rootUrl));
        }

        private string _rootUrl;
        [CanBeNull] private string _apiKey;
        [CanBeNull] private IRetryStrategy _retryStrategy = new LinearRetryStrategy();
        [CanBeNull] private ICachingStrategy _cachingStrategy = new AttributeBasedCachingStrategy();
        [CanBeNull] private TimeSpan? _timeout = null;
        private List<ICallsWrapper> _additionalCallsWrappers = new List<ICallsWrapper>();
        private List<DelegatingHandler> _additionalDelegatingHandlers = new List<DelegatingHandler>();
        private JsonSerializerSettings _jsonSerializerSettings;
        private IUrlParameterFormatter _urlParameterFormatter = new LykkeDefaultUrlParameterFormatter();

        /// <summary>
        /// Specifies the value of the api-key header to add to the requests.
        /// If not called - no api-key is added. 
        /// </summary>
        public HttpClientGeneratorBuilder WithApiKey([NotNull] string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            return this;
        }

        /// <summary>
        /// Sets the retry stategy used to handle requests failures. If not called - the default one is used.
        /// </summary>
        public HttpClientGeneratorBuilder WithRetriesStrategy([NotNull] IRetryStrategy retryStrategy)
        {
            _retryStrategy = retryStrategy ?? throw new ArgumentNullException(nameof(retryStrategy));
            return this;
        }

        /// <summary>
        /// Configures not to use retries
        /// </summary>
        public HttpClientGeneratorBuilder WithoutRetries()
        {
            _retryStrategy = null;
            return this;
        }

        /// <summary>
        /// Configures the caching strategy to use. If not called - the default one is used.
        /// </summary>
        public HttpClientGeneratorBuilder WithCachingStrategy([NotNull] ICachingStrategy cachingStrategy)
        {
            _cachingStrategy = cachingStrategy ?? throw new ArgumentNullException(nameof(cachingStrategy));
            return this;
        }

        /// <summary>
        /// Configures not to use methods results caching
        /// </summary>
        public HttpClientGeneratorBuilder WithoutCaching()
        {
            _cachingStrategy = null;
            return this;
        }

        /// <summary>
        /// Configures to use timeout.
        /// </summary>
        /// <exception cref="System.TimeoutException">Throws for any client's generated method</exception>
        public HttpClientGeneratorBuilder WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// Adds an additional method call wrapper
        /// </summary>
        public HttpClientGeneratorBuilder WithAdditionalCallsWrapper([NotNull] ICallsWrapper callsWrapper)
        {
            _additionalCallsWrappers.Add(callsWrapper ?? throw new ArgumentNullException(nameof(callsWrapper)));
            return this;
        }

        /// <summary>
        /// Adds an additional http delegating handler
        /// </summary>
        public HttpClientGeneratorBuilder WithAdditionalDelegatingHandler([NotNull] DelegatingHandler delegatingHandler)
        {
            _additionalDelegatingHandlers.Add(delegatingHandler ?? throw new ArgumentNullException(nameof(delegatingHandler)));
            return this;
        }

        /// <summary>
        /// Configure custom json serializer settings
        /// </summary>
        public HttpClientGeneratorBuilder WithJsonSerializerSettings([NotNull] JsonSerializerSettings settings)
        {
            _jsonSerializerSettings = settings;
            return this;
        }

        /// <summary>
        ///     Configure custom <see cref="IUrlParameterFormatter" /> for refit settings.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="urlParameterFormatter" /> is null.
        /// </exception>
        public HttpClientGeneratorBuilder WithUrlParameterFormatter(
            [NotNull] IUrlParameterFormatter urlParameterFormatter)
        {
            _urlParameterFormatter =
                urlParameterFormatter ?? throw new ArgumentNullException(nameof(urlParameterFormatter));
            return this;
        }

        /// <summary>
        /// Configure client to log request and response data if status code of response does not equal 2xx.
        /// </summary>
        /// <param name="logFactory">Factory to create instance of <see cref="ILog"/>.</param>
        /// <returns></returns>
        public HttpClientGeneratorBuilder WithRequestErrorLogging(
            [NotNull] ILogFactory logFactory)
        {
            var handler = new LogHttpRequestErrorHttpClientHandler(logFactory ?? throw new ArgumentNullException(nameof(logFactory)));
            _additionalDelegatingHandlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Creates the configured <see cref="HttpClientGenerator"/> instance
        /// </summary>
        public HttpClientGenerator Create()
        {
            return new HttpClientGenerator(_rootUrl, 
                GetCallsWrappers(null),
                GetDelegatingHandlers(),
                _jsonSerializerSettings, 
                _urlParameterFormatter);
        }

        /// <summary>
        /// Creates the configured <see cref="HttpClientGenerator"/> instance
        /// </summary>
        /// <param name="cacheManager">Instance of class which is responsible for clients cache invalidation only. 
        /// Use Lykke.HttpClientGenerator.Caching.ClientCacheManager</param>
        /// <returns></returns>
        public HttpClientGenerator Create(IClientCacheManager cacheManager)
        {
            return new HttpClientGenerator(_rootUrl,
                GetCallsWrappers(cacheManager), 
                GetDelegatingHandlers(), 
                _jsonSerializerSettings, 
                _urlParameterFormatter);
        }

        private IEnumerable<DelegatingHandler> GetDelegatingHandlers()
        {
            if (_timeout != null)
            {
                yield return new TimeoutHandler(_timeout.Value);
            }

            if (_additionalDelegatingHandlers != null)
            {
                foreach (var additionalDelegatingHandler in _additionalDelegatingHandlers)
                {
                    yield return additionalDelegatingHandler;
                }
            }

            if (_apiKey != null)
            {
                yield return new ApiKeyHeaderHttpClientHandler(_apiKey);
            }

            yield return new UserAgentHeaderHttpClientHandler(GetDefaultUserAgent());

            if (_retryStrategy != null)
            {
                yield return new RetryingHttpClientHandler(_retryStrategy);
            }
        }

        private IEnumerable<ICallsWrapper> GetCallsWrappers(IClientCacheManager cacheManager)
        {
            if (_additionalCallsWrappers != null)
            {
                foreach (var additionalCallsWrapper in _additionalCallsWrappers)
                {
                    yield return additionalCallsWrapper;
                }
            }

            if (_cachingStrategy != null)
            {
                var cacheProvider = new RemovableAsyncCacheProvider(new MemoryCache(new MemoryCacheOptions()));
                CachingCallsWrapper cachingCallsWrapper = new CachingCallsWrapper(_cachingStrategy, cacheProvider);
                if (cacheManager != null)
                {
                    cacheManager.OnInvalidate += cachingCallsWrapper.InvalidateCache;
                }

                yield return cachingCallsWrapper;
            }
        }

        private static string GetDefaultUserAgent()
        {
            return PlatformServices.Default.Application.ApplicationName + " v" +
                   PlatformServices.Default.Application.ApplicationVersion;
        }
    }
}