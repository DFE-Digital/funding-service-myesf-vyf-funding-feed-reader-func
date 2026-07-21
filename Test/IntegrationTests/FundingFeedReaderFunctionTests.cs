using Clients;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds_azurefunction_fundingfeedreader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApplicationInsightsLogger = Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLogger;

namespace Test.IntegrationTests
{
    [TestClass]
    public class FundingFeedReaderFunctionTests : BaseIntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingFeedReaderFunctionTests"/> class.
        /// </summary>
        public FundingFeedReaderFunctionTests()
        {
            SetEnvironmentVariables(true);

            CosmosDocumentClient = SetUpCosmosDocumentClient();
            FundingCollectionName = CosmosDocumentClient.FundingCollectionName;
            ProviderFundingCollectionName = CosmosDocumentClient.ProviderFundingCollectionName;
            DatabaseName = CosmosDocumentClient.DatabaseName;
        }

        [TestMethod, TestCategory("Integration")]
        public void RunHttp_IfVariablesAreNotSetup_EnsureExceptionThrown()
        {
            // Arrange
            SetEnvironmentVariables(false);

            var logger = GetLogger();

            // Act
            Func<Task> act = () => EntryPoints.RunHttp(GetHttpRequest().Object, logger);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Error: Fundings feed reader has missing configuration setting(s) - CosmosDbEndPoint");

            // Tidy-up
            Environment.SetEnvironmentVariable("auth:useAuthentication", "true");
        }

        /// <summary>
        /// Get Item count in the fundings collection.
        /// </summary>
        /// <returns>Number of items in collection.</returns>
        public async Task<long> GetFundingsCollectionItemCount()
        {
            var sql = $"SELECT VALUE COUNT(1) FROM c where IS_DEFINED(c.fundingStream) = true";

            var result = await CosmosDocumentClient.CreateDocumentQuery<long>(
                UriFactory.CreateDocumentCollectionUri(
                    CosmosDocumentClient.DatabaseName,
                    CosmosDocumentClient.FundingCollectionName),
                sql,
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }).ToListAsync();

            return result[0];
        }

        /// <summary>
        /// Get Item count in the provider fundings collection.
        /// </summary>
        /// <returns>Number of items in collection.</returns>
        public async Task<long> GetProviderFundingsCollectionItemCount()
        {
            var sql = $"SELECT VALUE COUNT(1) FROM c where IS_DEFINED(c.fundingStreamCode) = true";

            var result = await CosmosDocumentClient.CreateDocumentQuery<long>(
                UriFactory.CreateDocumentCollectionUri(
                    CosmosDocumentClient.DatabaseName,
                    CosmosDocumentClient.ProviderFundingCollectionName),
                sql,
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }).ToListAsync();

            return result[0];
        }

        private static Mock<HttpRequest> GetHttpRequest()
        {
            var query = new Mock<IQueryCollection>(MockBehavior.Strict);
            query.Setup(s => s["useBookmark"]).Returns("true");
            query.Setup(s => s["fundingStreamCodes"]).Returns("default");

            query.Setup(s => s.GetEnumerator()).Returns(new List<KeyValuePair<string, StringValues>>
            {
                new KeyValuePair<string, StringValues>("useBookmark", new StringValues("true")),
                new KeyValuePair<string, StringValues>("fundingstreamcodes", new StringValues("default"))
            }.GetEnumerator());

            query.Setup(s => s.ContainsKey(It.IsAny<string>())).Returns(true);

            var service = new Mock<HttpRequest>(MockBehavior.Strict);
            service.Setup(s => s.Query).Returns(query.Object);

            return service;
        }

        /// <summary>
        /// Setup CosmosDB Client.
        /// </summary>
        /// <returns>Instance of CosmosDocumentClient.</returns>
        private CosmosDocumentClient SetUpCosmosDocumentClient()
        {
            // Setup CosmosDb Client
            var cosmosDbEndPoint = Environment.GetEnvironmentVariable("cdb:endpointUri");
            var cosmosDbKey = Environment.GetEnvironmentVariable("cdb:endpointKey");
            var cosmosDbName = Environment.GetEnvironmentVariable("cdb:dbName");
            var cosmosFundingGroupCollectionName = Environment.GetEnvironmentVariable("cdb:fundingGroupCollectionName");
            var cosmosProviderFundingCollectionName =
                Environment.GetEnvironmentVariable("cdb:providerFundingCollectionName");
            var cosmosAuditCollectionName = Environment.GetEnvironmentVariable("cdb:auditCollectionName");
            var cosmosConnectionMode = Environment.GetEnvironmentVariable("cdb:ConnectionMode") ?? "Direct";

            return new CosmosDocumentClient(
                cosmosDbEndPoint,
                cosmosDbKey,
                cosmosDbName,
                cosmosFundingGroupCollectionName,
                cosmosProviderFundingCollectionName,
                cosmosAuditCollectionName,
                cosmosConnectionMode,
                null,
                null);
        }

        /// <summary>
        /// Create An Insights Logger that is injected by Azure Function.
        /// </summary>
        /// <returns>An instance of ILogger.</returns>
        private Microsoft.Extensions.Logging.ILogger GetLogger()
        {
            var configuration = Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.CreateDefault();
            var client = new Microsoft.ApplicationInsights.TelemetryClient(configuration);

            var options = new Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerOptions();
            return new ApplicationInsightsLogger("CN", client, options);
        }

        /// <summary>
        /// Setup all the required environment variables.
        /// </summary>
        /// <param name="allValid">If false the one setting will be set to null.</param>
        private void SetEnvironmentVariables(bool allValid)
        {
            var configFileToUse = "local.settings.json";

            if (!ConfigFileExists(configFileToUse))
            {
                configFileToUse = "cloud.settings.json";
            }

            // Configuration management.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFileToUse)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var configKeyValuePairs = configuration.AsEnumerable().Where(z => z.Key.Contains("Values:")).ToList();

            foreach (var configKeyValue in configKeyValuePairs)
            {
                Environment.SetEnvironmentVariable(configKeyValue.Key.Substring(7), configKeyValue.Value);
            }

            if (!allValid)
            {
                Environment.SetEnvironmentVariable("cdb:endpointUri", null);
            }
        }

        /// <summary>
        /// Check to see if specified file exists.
        /// </summary>
        /// <param name="fileName">Filename to check for.</param>
        /// <returns>True if file found.</returns>
        private bool ConfigFileExists(string fileName)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] fileInfo = dirInfo.GetFiles(fileName);

            return fileInfo.Length != 0;
        }
    }
}