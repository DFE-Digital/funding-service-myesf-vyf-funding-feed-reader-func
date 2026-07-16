using ApplicationLogger;
using Clients;
using CorporateSchema.Version4_00;
using Domain;
using Domain.Messages;
using Domain.Models;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.UnitTests
{
    [TestClass]
    public class FundingFeedReaderTests
    {
        #region Test Constants

        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>(MockBehavior.Strict);
        private readonly Mock<IHttpService> _mockHttpService = new Mock<IHttpService>(MockBehavior.Strict);
        private readonly Mock<ICosmosDocumentClient> _mockDocumentClient = new Mock<ICosmosDocumentClient>(MockBehavior.Strict);
        private readonly Mock<IAzureSearchServiceManager> _mockAzureSearchServiceManager = new Mock<IAzureSearchServiceManager>(MockBehavior.Strict);
        private readonly Uri _expectedUriFunding = UriFactory.CreateDocumentCollectionUri("IGNORED", "FUNDING");
        private readonly Uri _expectedUriProviderFunding = UriFactory.CreateDocumentCollectionUri("IGNORED", "PROVIDERFUNDING");

        #endregion


        #region Tests

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundings_BasicMock_RunsWithoutError()
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader();

            // Act
            await fundingsFeedReader.Process("test", false);
        }

        [DataRow(0, 0)]
        [DataRow(1, 0)]
        [DataRow(2, 0)]
        [DataRow(999, 0)]
        [DataRow(1, 1)]
        [DataRow(2, 10)]
        [DataRow(999, 1)]
        [DataRow(1, 999)]
        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundings_ProcessesNFundingsAndMProviderFundings_UpsertsNFundingsAndMProviderFundings(
            int fundingsCount, int providerFundersPerFundingCount)
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(fundingsCount, providerFundersPerFundingCount);
            var expectedFundingsProviderCalls = providerFundersPerFundingCount * fundingsCount;

            // Act
            await fundingsFeedReader.Process("test", false);

            // Assert
            _mockDocumentClient.Verify(
                property => property.UpsertDocumentAsync(
                    _expectedUriFunding,
                    It.IsAny<object>()),
                Times.Exactly(fundingsCount));

            _mockDocumentClient.Verify(
                property => property.UpsertDocumentAsync(
                    _expectedUriProviderFunding,
                    It.IsAny<object>()),
                Times.Exactly(expectedFundingsProviderCalls));
        }

        [DataRow(1, 0, 0)]
        [DataRow(2, 0, 0)]
        [DataRow(3, 0, 0)]
        [DataRow(15, 0, 0)]
        [DataRow(2, 2, 0)]
        [DataRow(2, 2, 2)]
        [DataRow(2, 2, 2)]
        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundings_WithNPagesAndMFundingsAndOProviderFundings_RequestsNPageAndMFundingsAndOProviderFundings(
            int pageCount, int fundingsPerPageCount, int providerFundersPerFundingCount)
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(fundingsPerPageCount, providerFundersPerFundingCount, pageCount);

            // Act
            await fundingsFeedReader.Process("test", false);

            // Assert
            _mockHttpService.Verify(property => property.GetAsync(It.Is<string>(s => s.Contains("PAGE") || s.Contains("/api/v4/statements/funding/notifications"))), Times.Exactly(pageCount));
        }

        [DataRow(2, 2, 0, 2)]
        [DataRow(2, 2, 2, 6)]
        [TestMethod, TestCategory("Unit")]
        public async Task ProcessFundingsWithPageSizeSetToOneAndEnsureOnlyOnePageIsProcessed(
            int pageCount, int fundingsPerPageCount, int providerFundersPerFundingCount, int calls)
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(fundingsPerPageCount, providerFundersPerFundingCount, pageCount);

            // Act
            await fundingsFeedReader.Process("test", false);

            // Assert
            _mockHttpService.Verify(
                property => property.GetAsync(It.Is<string>(s => s.Contains("PAGE") || s.Contains("/api/v4/statements/funding/"))), Times.Exactly(calls));
        }

        [TestMethod, TestCategory("Unit")]
        public void EnsureExceptionThrownIfDocumentCannotBeUploaded()
        {
            // Arrange
            var fundingsFeedReader = GetFundingsFeedReader(100, 0);

            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>()))
                .Throws(It.IsAny<Exception>());

            // Act
            Func<Task> result = () => fundingsFeedReader.Process("test", false);

            // Assert
            result.Should().Throw<Exception>();
        }

        #endregion


        #region Helper Methods

        private FundingsFeedReader GetFundingsFeedReader(int fundingsCount = 2, int providerFundingsCountPerFunding = 0, int pageCount = 1)
        {
            var mockDocumentClient = SetupMockDocumentClient(fundingsCount, providerFundingsCountPerFunding);
            var mockHttpService = SetupMockHttpService(fundingsCount, providerFundingsCountPerFunding, pageCount);
            var mockLogger = SetupMockLogger();
            var mockAzureSearchServiceManager = SetupMockAzureSearchServiceManager();

            var feedUri = $"http://www.example.org/api/v4/statements/funding/notifications?pageSize=10";
            var providerFundingUri = $"http://www.example.org/api/v4/statements/funding/provider";
            var fundingLookupUri = $"http://www.example.org/api/v4/statements/funding/byId";
            var providerFundingEnhancementsUri = $"http://www.example.org/api/v4/statements/providerenchancements";

            var simultanousCosmosReadWriteCount = 100;

            return new FundingsFeedReader(
                feedUri,
                fundingLookupUri,
                providerFundingUri,
                providerFundingEnhancementsUri,
                mockHttpService.Object,
                mockDocumentClient.Object,
                mockAzureSearchServiceManager.Object,
                mockLogger.Object,
                null,
                simultanousCosmosReadWriteCount,
                3,
                new FeedReaderResultReport(),
                int.MaxValue);
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
                fundings.Add(GetAtomEntry(idx + 1, $"Id{idx + 1}", providerFundingsCountPerFunding, idx + 1));
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
                        Rel = "prev-archive",
                        Href = $"PAGE{pageCount - 1}"
                    }
                };
            }

            var funding = feedResponseModel.AtomEntry.FirstOrDefault()?.Content.Funding;
            var enchancements = funding == null ? new List<FundingIdObject>() : new List<FundingIdObject>
            {
                new FundingIdObject
                {
                    FundingId = funding.Id
                }
            };

            _mockHttpService
                .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("/api/v4/statements/funding/notifications"))))
                .ReturnsAsync(JsonConvert.SerializeObject(feedResponseModel));

            _mockHttpService
                .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("/api/v4/statements/funding/byId"))))
                .ReturnsAsync(JsonConvert.SerializeObject(funding));

            _mockHttpService
                .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("/api/v4/statements/providerenchancements"))))
                .ReturnsAsync(JsonConvert.SerializeObject(enchancements));

            _mockHttpService
                .Setup(property => property.GetAsync(It.Is<string>(s => s.Contains("/api/v4/statements/funding/provider"))))
                .ReturnsAsync(JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {
                        "provider", JObject.FromObject(new Provider
                        {
                            OtherIdentifiers = new List<ProviderIdentifier>
                            {
                                new ProviderIdentifier
                                {
                                    Type = "UKPRN",
                                    Value = "123"
                                },
                            },
                            ProviderDetails = new ProviderDetails()
                            {
                                LocalAuthorityName = "LA Name"
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
                    Rel = "prev-archive",
                    Href = $"PAGE{pageNumber - 1}"
                });
            }

            return JsonConvert.SerializeObject(model);
        }

        private FeedResponseContentModel GetAtomEntry(int instance, string id, int providerFundingsCountPerFunding, int version)
        {
            var providerFundings = new List<string>();

            for (var idx = 0; idx < providerFundingsCountPerFunding; idx++)
            {
                providerFundings.Add($"{id}_Id{idx + 1}");
            }

            return new FeedResponseContentModel
            {
                Id = id,
                Content = new FeedBaseModel
                {
                    Funding = new FundingFeed
                    {
                        Id = "--Payment-LocalAuthority--" + instance,
                        FundingStream = new FundingStream(),
                        FundingPeriod = new FundingPeriod(),
                        OrganisationGroup = new OrganisationGroup(),
                        ProviderFundings = providerFundings,
                        FundingVersion = version.ToString()
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
            var getCountSql = "SELECT VALUE COUNT(1) FROM c where IS_DEFINED(c.fundingStream) = true";
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

                fundingsExistSql += $@"c.id = ""--Payment-LocalAuthority--{idx + 1}""";
            }

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<string>(_expectedUriFunding, fundingsExistSql, It.IsAny<FeedOptions>()))
                .Returns(new List<string>().AsQueryable());

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<string>(_expectedUriFunding, "SELECT value c.id FROM c", It.IsAny<FeedOptions>()))
                .Returns(new List<string>().AsQueryable());
        }

        private void SetupProviderFundingDocumentClient(int providerFundingsCountPerFunding)
        {
            var getCountSql = "SELECT VALUE COUNT(1) FROM c where IS_DEFINED(c.fundingStreamCode) = true";
            var expectedCount = new List<long> { 1 }.AsQueryable();

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<long>(_expectedUriProviderFunding, getCountSql, It.IsAny<FeedOptions>()))
                .Returns(expectedCount);

            var p = new List<ParentEnrichment>().AsQueryable();

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<ParentEnrichment>(
                    _expectedUriProviderFunding,
                    It.IsAny<string>(),
                    It.IsAny<FeedOptions>()))
                .Returns(p);

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
                .Setup(property => property.CreateDocumentQuery<string>(
                    _expectedUriProviderFunding,
                    providerFundingsExistSql,
                    It.IsAny<FeedOptions>()))
                .Returns(new List<string>().AsQueryable());

            _mockDocumentClient
                .Setup(property => property.CreateDocumentQuery<string>(
                    _expectedUriProviderFunding,
                    "SELECT value c.id FROM c",
                    It.IsAny<FeedOptions>()))
                .Returns(new List<string>().AsQueryable());
        }

        private Mock<IAzureSearchServiceManager> SetupMockAzureSearchServiceManager()
        {
            var fundingIndexerName = "funding indexer name";

            _mockAzureSearchServiceManager.Setup(x => x.GetAllIndexerNames()).ReturnsAsync(new List<string>() { "other indexer name", fundingIndexerName });
            _mockAzureSearchServiceManager.Setup(x => x.RunIndexer(fundingIndexerName)).ReturnsAsync(true);

            return _mockAzureSearchServiceManager;
        }

        #endregion
    }
}
