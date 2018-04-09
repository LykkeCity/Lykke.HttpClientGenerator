using System;
using System.Reflection;

namespace Lykke.ClientGenerator.Caching
{
    public class AttributeBasedCachingCallsWrapper : CachingCallsWrapper
    {
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