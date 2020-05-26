using System;
using System.Reflection;

namespace Lykke.HttpClientGenerator
{
    internal static class ExceptionExtensions
    {
        private static readonly FieldInfo ExceptionMessageField =
            typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void SetMessage(this Exception exception, string message)
        {
            ExceptionMessageField.SetValue(exception, message);
        }
    }
}