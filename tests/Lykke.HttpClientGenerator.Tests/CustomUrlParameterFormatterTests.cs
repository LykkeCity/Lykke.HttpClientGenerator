using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.HttpClientGenerator.Infrastructure;
using NUnit.Framework;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    [TestFixture]
    public class CustomUrlParameterFormatterTests
    {
        private const string TestHost = "http://fake.host";

        [Theory]
        [TestCase("hello")]
        [TestCase("test")]
        public async Task
            HttpClientGeneratorConstructor_WhenUrlParameterFormatterIsNull_ShouldUseDefaultUrlParameterFormatter(
                string urlParam)
        {
            // Arrange
            var handler = CreateUriHandler();

            // Act
            var result = await new HttpClientGenerator(TestHost, new ICallsWrapper[0],
                    new[] {handler},
                    null,
                    null)
                .Generate<IUrlFormatterTestApi>()
                .Test(urlParam);

            // Assert
            result.Should().BeEquivalentTo($"{TestHost}/fake/{urlParam}/");
        }

        [Theory]
        [TestCase("hello", "world")]
        [TestCase("test", "suffix")]
        public async Task
            HttpClientGeneratorConstructor_WithCustomUrlParameterFormatter_ShouldUseCustomUrlParameterFormatter(
                string urlParam, string suffix)
        {
            // Arrange
            var handler = CreateUriHandler();

            // Act
            var result = await new HttpClientGenerator(TestHost, new ICallsWrapper[0],
                    new[] {handler},
                    null,
                    new SuffixUrlParameterFormatter(suffix))
                .Generate<IUrlFormatterTestApi>()
                .Test(urlParam);

            // Assert
            result.Should().BeEquivalentTo($"{TestHost}/fake/{urlParam}_{suffix}/");
        }

        private DelegatingHandler CreateUriHandler()
        {
            return new FakeHttpClientHandler(request =>
            {
                var requestUri = request.RequestUri.ToString();

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(requestUri)
                };
            });
        }
    }

    internal interface IUrlFormatterTestApi
    {
        [Get("/fake/{urlParam}/")]
        Task<string> Test(string urlParam);
    }

    internal class SuffixUrlParameterFormatter : IUrlParameterFormatter
    {
        private readonly string _suffix;

        public SuffixUrlParameterFormatter(string suffix)
        {
            _suffix = suffix ?? string.Empty;
        }

        public string Format(object value, ParameterInfo parameterInfo)
        {
            return string.Concat(value, '_', _suffix);
        }
    }
}