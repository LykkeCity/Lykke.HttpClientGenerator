using System;
using System.Reflection;

namespace Lykke.HttpClientGenerator
{
    /// <summary>
    /// Extension methods for <see cref="Exception"/> 
    /// </summary>
    public static class ExceptionExtensions
    {
        private static readonly FieldInfo ExceptionMessageField =
            typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void SetMessage(this Exception exception, string message)
        {
            ExceptionMessageField.SetValue(exception, message);
        }
    }
}