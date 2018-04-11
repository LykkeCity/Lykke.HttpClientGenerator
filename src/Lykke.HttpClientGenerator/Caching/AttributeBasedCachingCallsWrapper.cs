using System;
using System.Reflection;

namespace Lykke.HttpClientGenerator.Caching
{
    /// <summary>
    /// Adds caching to method calls. The caching TimeSpan is extracted from <see cref="ClientCachingAttribute"/>,
    /// applied to the executing method.
    /// </summary>
    public class AttributeBasedCachingCallsWrapper : CachingCallsWrapper
    {
        /// <inheritdoc />
        protected override TimeSpan GetCachingTime(MethodInfo targetMethod, object[] args)
        {
            var clientCachingAttribute = targetMethod.GetCustomAttribute<ClientCachingAttribute>();
            var attributeCachingTime = clientCachingAttribute?.CachingTime;
            return attributeCachingTime == null || attributeCachingTime < TimeSpan.Zero 
                ? TimeSpan.Zero
                : attributeCachingTime.Value;
        }
    }
}