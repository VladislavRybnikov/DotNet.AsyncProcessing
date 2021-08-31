using System;

namespace DotNet.AsyncProcessing.Exceptions
{
    /// <summary>
    /// Consumer exception.
    /// </summary>
    public class ConsumerException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">Actual exception.</param>
        public ConsumerException(Exception innerException)
            : base(
                $"Consumer exception. See inner exception '{innerException?.GetType().Name}' for more details",
                innerException)
        { }
    }
}