using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;

namespace ApplicationLogger
{
    /// <summary>
    /// Log event(s) to Application Insights.
    /// </summary>
    public class ApplicationInsightsLogger : ILogger
    {
        private readonly string _connectionString;
        private readonly TelemetryClient _telemetryClient;
        private readonly IDictionary<string, string> _properties;
        private readonly bool _outputToConsole;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsLogger" /> class.
        /// </summary>
        /// <param name="connectionString">connection to access Azure Application Insights.</param>
        /// <param name="properties">properties for setting up Application Insights.</param>
        /// <param name="outputToConsole">Show output on console.</param>
        public ApplicationInsightsLogger(string connectionString, IDictionary<string, string> properties, bool outputToConsole)
        {
            _connectionString = connectionString;
            var config = new TelemetryConfiguration { ConnectionString = _connectionString };
            _telemetryClient = new TelemetryClient(config);

            _properties = properties;
            _outputToConsole = outputToConsole;
        }

        #endregion


        #region Logging Methods

        /// <summary>
        /// Log application events to Application Insights.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="category">Category to assign to log.</param>
        /// <param name="severityLevel">Severity of logging details.</param>
        public void LogTrace(string message, Category category = Category.FundingFeedReader, Severity severityLevel = Severity.Information)
        {
            IDictionary<string, string> properties = new Dictionary<string, string>(_properties);
            try
            {
                properties.Add(new KeyValuePair<string, string>("DomainArea", category.ToString()));
                _telemetryClient.TrackTrace(message, (Microsoft.ApplicationInsights.DataContracts.SeverityLevel)severityLevel, properties);

                if (_outputToConsole)
                {
                    Console.WriteLine(message);
                }
            }
            catch
            {
                if (_outputToConsole)
                {
                    Console.WriteLine("Error logging to Application Insights");
                }

                throw;
            }
        }

        /// <summary>
        /// log an application exception to Application Insights.
        /// </summary>
        /// <param name="exception">Exception thrown by application.</param>
        public void LogException(Exception exception)
        {
            try
            {
                try
                {
                    _telemetryClient.TrackException(exception, _properties);

                    if (_outputToConsole)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex, _properties);
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// log an application exception to Application Insights.
        /// </summary>
        /// <param name="exception">Exception thrown by application.</param>
        /// <param name="additionalMessage">Additional information associated with exception.</param>
        public void LogException(Exception exception, string additionalMessage)
        {
            const string messageKey = "Message";
            string newMessage = $"{additionalMessage}; ";

            var dictionary = _properties;

            if (dictionary.ContainsKey(messageKey))
            {
                dictionary[messageKey] += newMessage;
            }
            else
            {
                dictionary.Add(messageKey, newMessage);
            }

            _telemetryClient.TrackException(exception, dictionary);

            if (_outputToConsole)
            {
                Console.WriteLine(exception.Message);
            }
        }

        #endregion
    }
}