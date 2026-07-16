using Clients;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds_azurefunction_fundingfeedreader;
using Pds_azurefunction_fundingfeedreader.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test.UnitTests
{
    [TestClass]
    public class FundingFeedReaderFunctionTests
    {
        private readonly string _feedData =
            "{\r\n    \"id\":\"8ab3321052cf4ae29d345f557d7ad8c5\",\r\n    \"title\":\"Calculate Funding Service Funding Feed\",\r\n    \"author\":{\r\n                 \"name\":\"Calculate Funding Service\",\r\n                 \"email\":\"calculate-funding@education.gov.uk\"\r\n               },\r\n    \"updated\":\"2021-03-17T21:15:58.8104348+00:00\",\r\n    \"rights\":\"calculate-funding@education.gov.uk\",\r\n    \"link\": [\r\n    {\r\n        \"href\":\"https://pp-api-calculate-funding.education.gov.uk/api/v4/statements/funding/notifications/38673?pageSize=1\",\r\n        \"rel\":\"prev-archive\"\r\n    }\r\n,\r\n    {\r\n        \"href\":\"https://pp-api-calculate-funding.education.gov.uk/api/v4/statements/funding/notifications?pageSize=1\",\r\n        \"rel\":\"self\"\r\n    }\r\n             ],\r\n    \"atomEntry\": []}\r\n";

        [TestMethod, TestCategory("Unit")]
        public async Task Process_UrlsNotConfigured_ArgumentNullExceptionThrown()
        {
            var httpMocked = new Mock<IHttpService>(MockBehavior.Strict);
            httpMocked.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(string.Empty));

            // Act
            Func<Task> act = async () => await EntryPoints.Process(null, null, null, null, null, httpMocked.Object, null, 0);

            // Asert
            await act.Should().ThrowAsync<ArgumentNullException>("Fundings API cannot be null");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Process_CosmosAndHttpServiceMocked_NoExceptionThrown()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "cdb:endpointUri", "https://example.documents.azure.com:443/" },
                { "cdb:endpointKey", "l1HJ02F6gkS7PGC0lsyVG2l5OY87X1QvQVOVKJIQisC3QGdE7qwixM4A1KMcEh2Q2ZFqc5nSg79NtSIaCaOXhw==" }, // This is made up
                { "cdb:dbName", "abc" },
                { "cdb:fundingGroupCollectionName", "cde" },
                { "cdb:providerFundingCollectionName", "fgh" },
                { "cdb:auditCollectionName", "ijk" },
                { "fundingsApi:baseUrl", "http://example.org" }
            };

            foreach (var setting in settings)
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }

            IEnvironmentVariablesModel localSettingsModel = new EnvironmentVariablesModel();

            var cosmosConfMocked = new Mock<ICosmosDbConfiguration>();
            cosmosConfMocked.Setup(x => x.ApplyNewCollectionThroughputAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult((int?)100));

            var cosmosMocked = new Mock<ICosmosDocumentClient>();
            cosmosMocked.Setup(x => x.DatabaseName).Returns("A");
            cosmosMocked.Setup(x => x.AuditCollectionName).Returns("B");
            cosmosMocked.Setup(x => x.FundingCollectionName).Returns("C");
            cosmosMocked.Setup(x => x.ProviderFundingCollectionName).Returns("D");

            var mockAzureSearchServiceManager = new Mock<IAzureSearchServiceManager>(MockBehavior.Strict);
            mockAzureSearchServiceManager.Setup(x => x.GetAllIndexerNames()).ReturnsAsync(new List<string>() { "other indexer name", "funding indexer name" });
            mockAzureSearchServiceManager.Setup(x => x.RunIndexer("funding indexer name")).ReturnsAsync(true);

            var httpMocked = new Mock<IHttpService>(MockBehavior.Strict);
            httpMocked.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(_feedData));

            // Act
            Func<Task> act = async () => await EntryPoints.Process(
                null,
                null,
                cosmosMocked.Object,
                mockAzureSearchServiceManager.Object,
                cosmosConfMocked.Object,
                httpMocked.Object,
                localSettingsModel,
                0);

            // Asert
            await act.Should().NotThrowAsync();
        }
    }
}