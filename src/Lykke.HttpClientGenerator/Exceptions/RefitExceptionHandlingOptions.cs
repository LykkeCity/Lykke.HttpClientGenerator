namespace Lykke.HttpClientGenerator.Exceptions
{
    /// <summary>
    /// Options for Refit exceptions handling middleware
    /// </summary>
    public class RefitExceptionHandlingOptions
    {
        /// <summary>
        /// If exception has to be thrown after processing
        /// </summary>
        public bool ReThrow { get; set; }
    }
}