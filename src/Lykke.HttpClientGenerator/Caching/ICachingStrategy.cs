using System;
using System.Reflection;

namespace Lykke.HttpClientGenerator.Caching
{
    /// <summary>
    /// Specifies the method results caching strategy
    /// </summary>
    public interface ICachingStrategy
    {
        /// <summary>
        /// Gets the amount of time for which the result of <paramref name="targetMethod"/>
        /// executed with <paramref name="args"/> will be cached.
        /// </summary>
        TimeSpan GetCachingTime(MethodInfo targetMethod, object[] args);
    }
}