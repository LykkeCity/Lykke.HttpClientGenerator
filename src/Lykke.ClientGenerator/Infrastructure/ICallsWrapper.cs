using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Lykke.ClientGenerator.Infrastructure
{
    public interface ICallsWrapper
    {
        Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler);
    }
}