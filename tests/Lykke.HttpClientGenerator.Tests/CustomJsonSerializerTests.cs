using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.HttpClientGenerator.Caching;
using Lykke.HttpClientGenerator.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Refit;

namespace Lykke.HttpClientGenerator.Tests
{
    public class CustomJsonSerializerTests
    {
        [Test]
        public async Task ShouldUseCustomJsonSerializer()
        {
            var list = new List<BaseTest>
            {
                new TestA {Name = "abc"},
                new TestB {Value = 100m}
            };

            var handler = new FakeHttpClientHandler(r =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(list)))
                });

            // act
            var result = await new HttpClientGenerator("http://fake.host", new ICallsWrapper[0],
                    new DelegatingHandler[] { handler },
                    new JsonSerializerSettings { Converters = new List<JsonConverter> { new CustomJsonConverter() } },
                    null)
                .Generate<IJsonTestInterface>()
                .Test();

            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public async Task ShouldUseDefaultConverterForOtherClasses()
        {
            var list = new List<CommonTest>
            {
                new CommonTest {Temp = "abc"},
                new CommonTest {Temp = "qqq"}
            };

            var handler = new FakeHttpClientHandler(r =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(list)))
                });

            // act
            var result = await new HttpClientGenerator("http://fake.host", new ICallsWrapper[0],
                    new DelegatingHandler[] { handler },
                    new JsonSerializerSettings { Converters = new List<JsonConverter> { new CustomJsonConverter() } },
                    null)
                .Generate<IJsonTestInterface>()
                .TestOnlyOne();

            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public async Task ShouldWorkWithoutCustomDeserializer()
        {
            var list = new List<CommonTest>
            {
                new CommonTest {Temp = "abc"},
                new CommonTest {Temp = "qqq"}
            };

            var handler = new FakeHttpClientHandler(r =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(list)))
                });

            // act
            var result = await new HttpClientGenerator("http://fake.host", new ICallsWrapper[0],
                    new DelegatingHandler[] { handler })
                .Generate<IJsonTestInterface>()
                .TestOnlyOne();

            CollectionAssert.AreEqual(list, result);
        }


        private class CustomJsonConverter : JsonConverter
        {
            /// <inheritdoc />
            public override bool CanConvert(Type objectType)
            {
                return typeof(BaseTest).IsAssignableFrom(objectType);
            }

            /// <inheritdoc />
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jsonObject = JObject.Load(reader);
                var target = Create(jsonObject);
                serializer.Populate(jsonObject.CreateReader(), target);
                return target;
            }

            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            private static BaseTest Create(JObject jsonObject)
            {
                switch (jsonObject["Type"].ToString())
                {
                    case "TestA":
                        return new TestA();
                    case "TestB":
                        return new TestB();
                }

                return null;
            }
        }

    }

    public interface IJsonTestInterface
    {
        [Get("/fake/url/")]
        Task<List<BaseTest>> Test();

        [Get("/fake2/url/")]
        Task<List<CommonTest>> TestOnlyOne();
    }

    public abstract class BaseTest
    {
        public abstract string Type { get; }
    }

    public class TestA : BaseTest
    {
        public string Name { get; set; }

        public override string Type => "TestA";

        public override bool Equals(object obj)
        {
            if (!(obj is TestA other))
                return false;
            return Name == other.Name;
        }
    }

    public class TestB : BaseTest
    {
        public decimal Value { get; set; }

        public override string Type => "TestB";

        public override bool Equals(object obj)
        {
            if (!(obj is TestB other))
                return false;
            return Value == other.Value;
        }
    }

    public class CommonTest
    {
        public string Temp { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is CommonTest other))
                return false;
            return Temp == other.Temp;
        }
    }
}
