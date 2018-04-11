using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Lykke.HttpClientGenerator.Infrastructure
{
    /// <summary>
    /// Interface of a class which wraps method calls with some logic.
    /// </summary>
    public interface ICallsWrapper
    {
        /// <summary>
        /// Handles methods calls 
        /// </summary>
        /// <param name="targetMethod">Method info of the method being called</param>
        /// <param name="args">Method call args</param>
        /// <param name="innerHandler">Underlying handler</param>
        /// <returns>The execution result</returns>
        /// <remarks>
        /// Implementations should follow a specific pattern: first, add the pre-execution logic,
        /// second, call and await the <paramref name="innerHandler"/>, and third, add some post-execution logic. 
        /// </remarks>
        Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler);
    }
}