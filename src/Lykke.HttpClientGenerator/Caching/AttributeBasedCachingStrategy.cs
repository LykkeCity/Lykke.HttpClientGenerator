using System;
using System.Reflection;

namespace Lykke.HttpClientGenerator.Caching
{
    /// <summary>
    /// Specifies caching with times extracted from <see cref="ClientCachingAttribute"/>,
    /// applied to the executing method.
    /// </summary>
    public class AttributeBasedCachingStrategy : ICachingStrategy
    {
        /// <inheritdoc />
        public TimeSpan GetCachingTime(MethodInfo targetMethod, object[] args)
        {
            var clientCachingAttribute = targetMethod.GetCustomAttribute<ClientCachingAttribute>();
            // ReSharper disable once ConstantConditionalAccessQualifier
            var attributeCachingTime = clientCachingAttribute?.CachingTime;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return attributeCachingTime == null || attributeCachingTime < TimeSpan.Zero 
                ? TimeSpan.Zero
                : attributeCachingTime.Value;
        }
    }
}