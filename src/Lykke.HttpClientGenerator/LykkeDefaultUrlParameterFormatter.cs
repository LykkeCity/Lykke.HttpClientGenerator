using System;
using System.Linq;
using System.Reflection;
using Common;
using Lykke.Snow.Common.Extensions;
using Refit;

namespace Lykke.HttpClientGenerator
{
    public class LykkeDefaultUrlParameterFormatter : DefaultUrlParameterFormatter
    {
        public override string Format(object parameterValue, ICustomAttributeProvider attributeProvider, Type type)
        {
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                var queryAttribute = attributeProvider.GetCustomAttributes(typeof(QueryAttribute), true)
                    .OfType<QueryAttribute>()
                    .FirstOrDefault();

                if (queryAttribute == null)
                {
                    return ((DateTime?) parameterValue).ToIso8601Format();
                }
            }

            return base.Format(parameterValue, attributeProvider, type);
        }
    }
}