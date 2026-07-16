using ApplicationLogger;
using Clients;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Test.UnitTests
{
    [TestClass]
    public class CosmosDbConfigurationTests
    {
        private const int InitDbSetting = 0;
        private const int NewDbSetting = 1;
        private const int WaitTimeMs = 0;
        private const string DbName = "dbName";
        private const string CollectionName = "CName";
        private readonly int[] throughputSizes = new int[] { 10, 20 };


        #region Change CosmosDB Settings at collection level.

        [TestMethod, TestCategory("Unit")]
        public void ApplyNewCollectionThroughputAsync_ChangeThroughputSettingTurnedOff_EnsureCollectionThroughputIsNotChanged()
        {
            // Arrange
            var message =
                $"{CollectionName}: Handle case whilst setting throughput to {throughputSizes[NewDbSetting]}. Current = {throughputSizes[InitDbSetting]} {WaitTimeMs} ms";

            // mock logger
            var mockLogger = new Mock<ILogger>(MockBehavior.Strict);

            mockLogger.Setup(x => x.LogException(It.IsAny<Exception>(), It.IsAny<string>()));

            // mock document client
            var mockCosmosDb = new Mock<ICosmosDocumentClient>(MockBehavior.Strict);

            mockCosmosDb.Setup(x => x.GetCurrentThroughputForCollection(CollectionName))
                        .ReturnsAsync(throughputSizes[InitDbSetting]);

            mockCosmosDb.Setup(x => x.ChangeThroughputForCollection(throughputSizes[NewDbSetting], CollectionName))
                        .ReturnsAsync(false);

            var cosmosConfig = new CosmosDbConfiguration(mockCosmosDb.Object, mockLogger.Object, WaitTimeMs, true);

            // Act
            Func<Task> result = () => cosmosConfig.ApplyNewCollectionThroughputAsync(CollectionName, throughputSizes[NewDbSetting]);

            // Assert
            result.Should().Throw<Exception>()
                .WithMessage(message);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ApplyNewCollectionThroughputAsync_ChangeThroughputSettingTurnedOn_EnsureCollectionThroughputIsAppliedSuccessfully()
        {
            // Arrange
            var callbackCount = 0;
            var messageWaiting =
                $"{CollectionName}:  Waiting to scale throughput from {throughputSizes[InitDbSetting]} to {throughputSizes[NewDbSetting]}. Scale wait-time {WaitTimeMs} ms";
            var messageWaited =
                $"{CollectionName}: Scaled throughput up from {throughputSizes[InitDbSetting]} to {throughputSizes[NewDbSetting]}";

            // mock logger
            var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
            mockLogger.Setup(x => x.LogTrace(messageWaiting, Category.FundingFeedReader, Severity.Information));
            mockLogger.Setup(x => x.LogTrace(messageWaited, Category.FundingFeedReader, Severity.Information));

            // mock document client
            var mockCosmosDb = new Mock<ICosmosDocumentClient>(MockBehavior.Strict);
            mockCosmosDb.Setup(x => x.GetCurrentThroughputForCollection(CollectionName))
                .ReturnsAsync(() => throughputSizes[callbackCount])
                .Callback(() => ++callbackCount);
            mockCosmosDb.Setup(x => x.ChangeThroughputForCollection(throughputSizes[NewDbSetting], CollectionName))
                .ReturnsAsync(true);

            var cosmosConfig = new CosmosDbConfiguration(mockCosmosDb.Object, mockLogger.Object, WaitTimeMs, true);

            // Act
            var result = await cosmosConfig.ApplyNewCollectionThroughputAsync(CollectionName, throughputSizes[NewDbSetting]);

            // Assert
            result.Should().Be(throughputSizes[InitDbSetting]);
        }

        [TestMethod, TestCategory("Unit")]
        public void ApplyNewCollectionThroughputAsync_NewCollectionThroughputDoesNotMatchExpectedValue_EnsureExceptionIsThrown()
        {
            // Arrange
            var callbackCount = 0;
            var messageWaiting =
                $"{CollectionName}:  Waiting to scale throughput from {throughputSizes[InitDbSetting]} to {throughputSizes[NewDbSetting]}. Scale wait-time {WaitTimeMs} ms";
            var exceptionMessage =
                $"{CollectionName}: Unable to change throughput from {throughputSizes[InitDbSetting]} to {throughputSizes[NewDbSetting]} {WaitTimeMs} ms";

            // mock logger
            var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
            mockLogger.Setup(x => x.LogTrace(messageWaiting, Category.FundingFeedReader, Severity.Information));
            mockLogger.Setup(x => x.LogException(It.IsAny<Exception>(), It.IsAny<string>()));

            // mock document client
            var mockCosmosDb = new Mock<ICosmosDocumentClient>(MockBehavior.Strict);
            mockCosmosDb.Setup(x => x.GetCurrentThroughputForCollection(CollectionName))
                .ReturnsAsync(() =>
                {
                    if (callbackCount == 1)
                    {
                        return 99; // return unexpected size
                    }

                    return throughputSizes[callbackCount];
                })
                .Callback(() => ++callbackCount);

            mockCosmosDb.Setup(x => x.ChangeThroughputForCollection(throughputSizes[NewDbSetting], CollectionName))
                .ReturnsAsync(true);

            var cosmosConfig = new CosmosDbConfiguration(mockCosmosDb.Object, mockLogger.Object, WaitTimeMs, true);

            // Act
            Func<Task> result = () => cosmosConfig.ApplyNewCollectionThroughputAsync(CollectionName, throughputSizes[NewDbSetting]);

            // Assert
            result.Should().Throw<Exception>()
                .WithMessage(exceptionMessage);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ApplyNewCollectionThroughputAsync_ThroughputIsAlreadySetAtNewValue_EnsureNewCollectionThroughputNotApplied()
        {
            // Arrange
            const int throughPutSize = 10;

            // mock logger
            var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
            mockLogger.Setup(x => x.LogTrace($"{CollectionName}: Already at throughput {throughPutSize}", Category.FundingFeedReader, It.IsAny<Severity>()));

            // mock document client
            var mockCosmosDb = new Mock<ICosmosDocumentClient>(MockBehavior.Strict);
            var cosmosConfig = new CosmosDbConfiguration(mockCosmosDb.Object, mockLogger.Object, WaitTimeMs, true);

            mockCosmosDb.Setup(x => x.GetCurrentThroughputForCollection(CollectionName))
                .ReturnsAsync(throughPutSize);

            // Act
            var result = await cosmosConfig.ApplyNewCollectionThroughputAsync(CollectionName, throughPutSize);

            // Assert
            result.Should().Be(throughPutSize);
        }

        #endregion
    }
}