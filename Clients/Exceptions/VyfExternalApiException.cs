using System;

namespace Clients.Exceptions
{
    /// <summary>
    /// Exception class for VyfExternalApiService methods.
    /// </summary>
    public class VyfExternalApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VyfExternalApiException"/> class.
        /// </summary>
        public VyfExternalApiException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VyfExternalApiException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public VyfExternalApiException(string message) : base(message)
        {
        }
    }
}
