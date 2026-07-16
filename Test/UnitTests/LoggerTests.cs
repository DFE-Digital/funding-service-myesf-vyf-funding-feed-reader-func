using ApplicationLogger;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Test.UnitTests
{
    [TestClass]
    public class LoggerTests
    {
        private const string TestConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000";

        [DataRow(Severity.Information)]
        [DataRow(Severity.Critical)]
        [DataRow(Severity.Error)]
        [DataRow(Severity.Verbose)]
        [DataRow(Severity.Warning)]
        [TestMethod, TestCategory("Unit")]
        public void LogTrace_LogSuccessful(Severity severityLevel)
        {
            // Arrange
            var properties = new Dictionary<string, string>
            {
                { "propertyKey", "propertyValue" }
            };

            var applicationInsightsLogger = new ApplicationInsightsLogger(TestConnectionString, properties, true);

            // Act
            Action act = () => applicationInsightsLogger.LogTrace("Log trace message", Category.FundingFeedReader, severityLevel);

            // Assert
            act.Should().NotThrow();
        }

        [DataRow(Severity.Information)]
        [DataRow(Severity.Critical)]
        [DataRow(Severity.Error)]
        [DataRow(Severity.Verbose)]
        [DataRow(Severity.Warning)]
        [TestMethod, TestCategory("Unit")]
        public void LogTrace_LogFail(Severity severityLevel)
        {
            // Arrange
            var properties = new Dictionary<string, string>
            {
                { "DomainArea", "propertyValue" }
            };

            var applicationInsightsLogger = new ApplicationInsightsLogger(TestConnectionString, properties, true);

            // Act
            Action act = () => applicationInsightsLogger.LogTrace("Log trace message", Category.FundingFeedReader, severityLevel);

            // Assert
            act.Should().Throw<Exception>();
        }

        [TestMethod, TestCategory("Unit")]
        public void LogException_LogSuccessful()
        {
            // Arrange
            var properties = new Dictionary<string, string>
            {
                { "propertyKey", "propertyValue" }
            };

            var applicationInsightsLogger = new ApplicationInsightsLogger(TestConnectionString, properties, true);

            // Act
            Action act = () => applicationInsightsLogger.LogException(new Exception());

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod, TestCategory("Unit")]
        public void LogExceptionWithAdditionalMessage_LogSuccessful()
        {
            // Arrange
            var properties = new Dictionary<string, string>
            {
                { "propertyKey", "propertyValue" }
            };

            var applicationInsightsLogger = new ApplicationInsightsLogger(TestConnectionString, properties, true);

            // Act
            Action act = () => applicationInsightsLogger.LogException(new Exception(), "additionalMessage");

            // Assert
            act.Should().NotThrow();
        }
    }
}
