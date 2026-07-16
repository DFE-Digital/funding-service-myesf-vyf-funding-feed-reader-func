using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Azure.Documents.Client;
using Moq;
using FeedReader.Domain.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using ApplicationLogger;
using FluentAssertions;
using CorporateSchema.Version3_00;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FeedReader.Domain.Tests
{
    [TestClass]
    public class RootTests
    {
        #region Test Constants

        Mock<ILogger> _mockLogger = new Mock<ILogger>(MockBehavior.Strict);
        Mock<IHttpService> _mockHttpService = new Mock<IHttpService>(MockBehavior.Strict);
        Mock<ICosmosDocumentClient> _mockDocumentClient = new Mock<ICosmosDocumentClient>(MockBehavior.Strict);
        Uri _expectedUriFunding = UriFactory.CreateDocumentCollectionUri("IGNORED", "FUNDING");
        Uri _expectedUriProviderFunding = UriFactory.CreateDocumentCollectionUri("IGNORED", "PROVIDERFUNDING");

        #endregion


        #region Tests

        [DataRow("")]
        [DataRow("default")]
        [DataRow("recovery")]
        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundings_BasicMock_RunsWithoutError(string runMode)
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(runMode);

            // Act
            var result = await fundingsFeedReader.ProcessFundings();

            // Assert
            result.Should().NotBeNull();
        }

        [DataRow("default", 0, 0)]
        [DataRow("recovery", 0, 0)]
        [DataRow("default", 1, 0)]
        [DataRow("recovery", 1, 0)]
        [DataRow("default", 2, 0)]
        [DataRow("recovery", 2, 0)]
        [DataRow("default", 999, 0)]
        [DataRow("recovery", 999, 0)]
        [DataRow("default", 1, 1)]
        [DataRow("recovery", 1, 1)]
        [DataRow("default", 2, 10)]
        [DataRow("recovery", 2, 10)]
        [DataRow("default", 999, 1)]
        [DataRow("recovery", 999, 1)]
        [DataRow("default", 1, 999)]
        [DataRow("recovery", 1, 999)]
        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundings_ProcessesNFundingsAndMProviderFundings_UpsertsNFundingsAndMProviderFundings(string runMode, 
            int fundingsCount, int providerFundersPerFundingCount)
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(runMode, fundingsCount, providerFundersPerFundingCount);
            var expectedFundingsProviderCalls = providerFundersPerFundingCount * fundingsCount;

            // Act
            await fundingsFeedReader.ProcessFundings();

            // Assert
            _mockDocumentClient.Verify(property => property.UpsertDocumentAsync(_expectedUriFunding, It.IsAny<object>()), 
                Times.Exactly(fundingsCount));

            _mockDocumentClient.Verify(property => property.UpsertDocumentAsync(_expectedUriProviderFunding, It.IsAny<object>()), 
                Times.Exactly(expectedFundingsProviderCalls));
        }

        [DataRow("default", 1, 0, 0)]
        [DataRow("default", 2, 0, 0)]
        [DataRow("default", 3, 0, 0)]
        [DataRow("default", 15, 0, 0)]
        [DataRow("default", 2, 2, 0)]
        [DataRow("default", 2, 2, 2)]
        [DataRow("recovery", 1, 0, 0)]
        [DataRow("recovery", 2, 0, 0)]
        [DataRow("recovery", 3, 0, 0)]
        [DataRow("recovery", 15, 0, 0)]
        [DataRow("recovery", 2, 2, 0)]
        [DataRow("recovery", 2, 2, 2)]
        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundings_WithNPagesAndMFundingsAndOProviderFundings_RequestsNPageAndMFundingsAndOProviderFundings(string runMode,
            int pageCount, int fundingsPerPageCount, int providerFundersPerFundingCount)
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(runMode, fundingsPerPageCount, providerFundersPerFundingCount, pageCount);

            // Act
            await fundingsFeedReader.ProcessFundings();

            // Assert
            _mockHttpService.Verify(property => 
                property.GetAsync(It.Is<string>(s => s.Contains("PAGE") || s.Contains("/api/funding/feed"))), Times.Exactly(pageCount));
        }

        [TestMethod, TestCategory("Unit")]
        public void ProcessFundings_ChangeThrouputUpAndDown_ThroughputChangeRequestsRecieved()
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader("default", programaticallyChangeThroughput: true);

            Func<Task> act = async () => await fundingsFeedReader.ProcessFundings();

            // Assert
            act.Should().Throw<Exception>();

            _mockDocumentClient.Verify(property =>
                property.ChangeThroughputForCollection(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(4));
        }

        #endregion


        #region Helper Methods

        private FundingsFeedReader GetFundingsFeedReader(string runMode, int fundingsCount = 2, int providerFundingsCountPerFunding = 0, int pageCount = 1,
            bool programaticallyChangeThroughput = false)
        {
            var _mockDocumentClient = SetupMockDocumentClient(fundingsCount, providerFundingsCountPerFunding);
            var _mockHttpService = SetupMockHttpService(fundingsCount, providerFundingsCountPerFunding, pageCount);
            var _mockLogger = SetupMockLogger();
                        
            var cosmosDbConfiguration = new CosmosDbConfiguration(_mockDocumentClient.Object, _mockLogger.Object, 
                5000, 1, programaticallyChangeThroughput);

            return new FundingsFeedReader(_mockHttpService.Object, _mockDocumentClient.Object, cosmosDbConfiguration,
                _mockLogger.Object, "http://www.example.org", 10, 10, runMode, 0);
        }

        private Mock<ILogger> SetupMockLogger()
        {
            _mockLogger.Setup(property => property.LogTrace(It.IsAny<string>(), Category.FundingFeedReader, Severity.Information));

            _mockLogger.Setup(property => property.LogException(It.IsAny<Exception>(), It.IsAny<string>()));

            return _mockLogger;
        }

        private Mock<IHttpService> SetupMockHttpService(int fundingsCount, int providerFundingsCountPerFunding, int pageCount)
        {
            var fundings = new List<FeedResponseContentModel>();

            for (var idx = 0; idx < fundingsCount; idx++)
            {
                fundings.Add(GetAtomEntry($"Id{idx + 1}", providerFundingsCountPerFunding));
            }

            var feedResponseModel = new FeedResponseModel
            {
                AtomEntry = fundings.ToArray()
            };

            if (pageCount > 1)
            {
                var originalFeedResponseModel = feedResponseModel;

                _mockHttpService
                    .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("PAGE"))))
                    .ReturnsAsync((string url) => GetFeedResponseModelWithLinks(originalFeedResponseModel, url));

                // Deep copy
                feedResponseModel = JsonConvert.DeserializeObject<FeedResponseModel>(JsonConvert.SerializeObject(feedResponseModel));

                feedResponseModel.Link = new List<FeedLink>
                {
                    new FeedLink
                    {
                        Rel = "previous",
                        Href = $"PAGE{(pageCount - 2)}"
                    }
                };
            }

            _mockHttpService
                .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("/api/funding/feed"))))
                .ReturnsAsync(JsonConvert.SerializeObject(feedResponseModel));

            _mockHttpService
                .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("/api/funding/providerfunding"))))
                .ReturnsAsync(JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "provider", JObject.FromObject(new Provider
                        {
                            OtherIdentifiers = new List<ProviderIdentifier>
                            {
                                new ProviderIdentifier
                                {
                                    Type = ProviderTypeIdentifier.UKPRN,
                                    Value = "123"
                                }
                            }
                        })
                    },
                    {
                        "id", "1"
                    }
                }));

            return _mockHttpService;
        }

        private string GetFeedResponseModelWithLinks(FeedResponseModel model, string url)
        {
            model = JsonConvert.DeserializeObject<FeedResponseModel>(JsonConvert.SerializeObject(model));
            model.Link = new List<FeedLink>();

            var pageNumber = int.Parse(url.Replace("PAGE", string.Empty));

            if (pageNumber > 0)
            {
                model.Link.Add(new FeedLink
                {
                    Rel = "previous",
                    Href = $"PAGE{(pageNumber - 1)}"
                });
            }

            return JsonConvert.SerializeObject(model);
        }

        private FeedResponseContentModel GetAtomEntry(string id, int providerFundingsCountPerFunding)
        {
            var providerFundings = new List<string>();

            for (var idx = 0; idx < providerFundingsCountPerFunding; idx++)
            {
                providerFundings.Add($"Id{idx + 1}");
            }

            return new FeedResponseContentModel
            {
                Id = id,
                Content = new FeedBaseModel
                {
                    Funding = new FundingFeed
                    {
                        FundingStream = new FundingStream(),
                        FundingPeriod = new FundingPeriod(),
                        OrganisationGroup = new OrganisationGroup(),
                        ProviderFundings = providerFundings
                    }
                }
            };
        }

        private Mock<ICosmosDocumentClient> SetupMockDocumentClient(int fundingsCount, int providerFundingsCountPerFunding)
        {
            // Config
            _mockDocumentClient
                .Setup(property => property.DatabaseName)
                .Returns("IGNORED");

            _mockDocumentClient
                .Setup(property => property.FundingCollectionName)
                .Returns("FUNDING");

            _mockDocumentClient
                .Setup(property => property.ProviderFundingCollectionName)
                .Returns("PROVIDERFUNDING");

            _mockDocumentClient
                .Setup(property => property.AuditCollectionName)
                .Returns("AUDIT");

            // Fundings + provider fundings lookups
            SetupFundingDocumentClient(fundingsCount);
            SetupProviderFundingDocumentClient(providerFundingsCountPerFunding);

            // Upserts
            _mockDocumentClient
                .Setup(property => property.UpsertDocumentAsync(_expectedUriFunding, It.IsAny<object>()))
                .ReturnsAsync(new ResourceResponse<Microsoft.Azure.Documents.Document>());

            _mockDocumentClient
                .Setup(property => property.UpsertDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>()))
                .ReturnsAsync(new ResourceResponse<Microsoft.Azure.Documents.Document>());

            // Throughput
            _mockDocumentClient
                .Setup(property => property.GetCurrentThroughputForCollection(It.IsAny<string>()))
                .ReturnsAsync(500);

            _mockDocumentClient
                .Setup(property => property.ChangeThroughputForCollection(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            return _mockDocumentClient;
        }

        private void SetupFundingDocumentClient(int fundingsCount)
        {
            var getCountSql = "SELECT VALUE COUNT(1) FROM c";
            var expectedCount = new List<long> { 1 }.AsQueryable();

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<long>(_expectedUriFunding, getCountSql, It.IsAny<FeedOptions>()))
                .Returns(expectedCount);

            var fundingsExistSql = @"SELECT VALUE c.id FROM c where ";

            for (var idx = 0; idx < fundingsCount; idx++)
            {
                if (idx > 0)
                {
                    fundingsExistSql += " or ";
                }

                fundingsExistSql += $@"c.id = ""Id{idx + 1}""";
            }

            _mockDocumentClient
            .Setup(property => property.CreateDocumentQuery<string>(_expectedUriFunding, fundingsExistSql, It.IsAny<FeedOptions>()))
            .Returns(new List<string>().AsQueryable());
        }

        private void SetupProviderFundingDocumentClient(int providerFundingsCountPerFunding)
        {
            var getCountSql = "SELECT VALUE COUNT(1) FROM c";
            var expectedCount = new List<long> { 1 }.AsQueryable();

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<long>(_expectedUriProviderFunding, getCountSql, It.IsAny<FeedOptions>()))
                .Returns(expectedCount);

            var providerFundingsExistSql = @"SELECT VALUE c.id FROM c where ";

            for (var idx = 0; idx < providerFundingsCountPerFunding; idx++)
            {
                if (idx > 0)
                {
                    providerFundingsExistSql += " or ";
                }

                providerFundingsExistSql += $@"c.id = ""Id{idx + 1}""";
            }

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<string>(_expectedUriProviderFunding, providerFundingsExistSql, It.IsAny<FeedOptions>()))
                .Returns(new List<string>().AsQueryable());
        }

        #endregion
    }
}
