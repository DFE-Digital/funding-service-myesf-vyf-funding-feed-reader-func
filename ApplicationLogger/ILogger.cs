using System;

namespace ApplicationLogger
{
    /// <summary>
    /// Interface for logging application events.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">The message category.</param>
        /// <param name="severity">The log severity.</param>
        void LogTrace(string message, Category category = Category.FundingFeedReader, Severity severity = Severity.Information);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        void LogException(Exception exception);

        /// <summary>
        /// Logs an exception with additional detail.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="additionalMessage">Additional log detail.</param>
        void LogException(Exception exception, string additionalMessage);
    }
}
