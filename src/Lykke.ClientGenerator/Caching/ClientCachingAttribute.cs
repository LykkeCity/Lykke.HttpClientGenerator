using System;

namespace Lykke.ClientGenerator.Caching
{
    /// <summary>
    /// Specifies the amount of time to cache this method call.
    /// Used with <see cref="AttributeBasedCachingCallsWrapper"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientCachingAttribute : Attribute
    {
        private TimeSpan _cachingTime;

        /// <summary>
        /// Gets the amount of time specified
        /// </summary>
        public TimeSpan CachingTime => _cachingTime;

        /// <summary>
        /// Gets or sets the hours component
        /// </summary>
        public int Hours
        {
            set => _cachingTime = new TimeSpan(value, _cachingTime.Minutes, _cachingTime.Seconds);
            get => _cachingTime.Hours;
        }

        /// <summary>
        /// Gets or sets the minutes component
        /// </summary>
        public int Minutes
        {
            set => _cachingTime = new TimeSpan(_cachingTime.Hours, value, _cachingTime.Seconds);
            get => _cachingTime.Minutes;
        }

        /// <summary>
        /// Gets or sets the seconds component
        /// </summary>
        public int Seconds
        {
            set => _cachingTime = new TimeSpan(_cachingTime.Hours, _cachingTime.Minutes, value);
            get => _cachingTime.Seconds;
        }
    }
}