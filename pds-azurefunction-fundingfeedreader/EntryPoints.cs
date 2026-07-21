using ApplicationLogger;
using Clients;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Pds_azurefunction_fundingfeedreader.Enums;
using Pds_azurefunction_fundingfeedreader.Helpers;
using Pds_azurefunction_fundingfeedreader.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Pds_azurefunction_fundingfeedreader
{
    /// <summary>
    /// Funding's Feed Reader Azure function.
    /// Process fundings from CFS endpoint and persist fundings to CosmosDB.
    /// </summary>
    public static class EntryPoints
    {
        /// <summary>
        /// A service bus entry point.
        /// </summary>
        /// <param name="message">The service bus message.</param>
        /// <param name="log">The log to write to.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("FundingFeedReaderFunctionServiceBus")]
        public static async Task Run_ServiceBus(
            [ServiceBusTrigger("runfeedreader", Connection = "sb:connectionString")] ServiceBusInputMessage message,
            ILogger log)
        {
            await Run(
                    log,
                    message.FundingStreams,
                    message.FundingPeriods,
                    message.ScenarioIds,
                    message.Id,
                    message.StartDateTime,
                    trigger: RunTrigger.ServiceBus);
        }

        /// <summary>
        /// A http based entry point (used for local dev).
        /// </summary>
        /// <param name="req">Info about the HTTP request.</param>
        /// <param name="log">The log to write to.</param>
        /// <returns>An awaitable task.</returns>
        [FunctionName("FundingFeedReaderFunctionHttp")]
        public static async Task RunHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            var query = req.Query;
            var useBookmark = true;
            if (query.ContainsKey(nameof(useBookmark)))
            {
                useBookmark = Convert.ToBoolean(query[nameof(useBookmark)]);
            }

            await Run(
                    log,
                    Convert.ToString(query["fundingStreamCodes"]) ?? "default",
                    useBookMark: useBookmark,
                    trigger: RunTrigger.Http);
        }

        /// <summary>
        /// A timer based entry point.
        /// </summary>
        /// <param name="timer">Info about the timer request.</param>
        /// <param name="log">The log to write to.</param>
        /// <returns>An awaitable task.</returns>
        [FunctionName("FundingFeedReaderFunctionTimer")]
        public static async Task Run_Timer([TimerTrigger("%timerInterval%")] TimerInfo timer, ILogger log)
        {
            await Run(
                    log,
                    trigger: RunTrigger.Timer);
        }

        /// <summary>
        /// Process all fundings from the fundings API.
        /// </summary>
        /// <param name="logger">Application Insights logger.</param>
        /// <param name="originalLogger">The original logger.</param>
        /// <param name="cosmosDbClient">The cosmos DB client to use.</param>
        /// <param name="azureSearchServiceManager">The azure search service manager.</param>
        /// <param name="cosmosDbConfiguration">Configuration for the cosmos db client.</param>
        /// <param name="httpService">The http service to use to talk to the api.</param>
        /// <param name="environmentVariables">Local Environment setting to use.</param>
        /// <param name="desiredThroughPutSize">The throughput size.</param>
        /// <param name="fundingStream">Funding stream.</param>
        /// <param name="fundingPeriodsCommaSeperated">Funding periods, separated by commas (optional).</param>
        /// <param name="scenarioIdsCommaSeperated">Scenario IDs to use, separated by commas (optional).</param>
        /// <param name="id">The ID of the audit history (optional).</param>
        /// <param name="startDateTimeString">The start date time for the audit (optional).</param>
        /// <param name="maximumPagesToProcess">Maximum pages to process (default is basically unlimited).</param>
        /// <param name="useBookMark">Use bookmark functionality.</param>
        /// <param name="trigger">The trigger method of feed reader. Http request or Timer.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task Process(
            ApplicationLogger.ILogger logger,
            ILogger originalLogger,
            ICosmosDocumentClient cosmosDbClient,
            IAzureSearchServiceManager azureSearchServiceManager,
            ICosmosDbConfiguration cosmosDbConfiguration,
            IHttpService httpService,
            IEnvironmentVariablesModel environmentVariables,
            int desiredThroughPutSize,
            string fundingStream = null,
            string fundingPeriodsCommaSeperated = null,
            string scenarioIdsCommaSeperated = null,
            string id = null,
            string startDateTimeString = null,
            int maximumPagesToProcess = int.MaxValue,
            bool useBookMark = true,
            RunTrigger trigger = RunTrigger.None)
        {
            if (httpService == null)
            {
                throw new ArgumentException("Http Service is null");
            }

            if (string.IsNullOrEmpty(environmentVariables?.FundingsApiUri))
            {
                throw new ArgumentNullException("Fundings API cannot be null");
            }

            fundingStream = environmentVariables.FundingsApiUri.IsMockUri() ? null : fundingStream;

            IFeedReaderInputUriModel feedReaderInputUriModel = new FeedReaderInputUriModel(
                                environmentVariables.FundingsApiUri,
                                environmentVariables.NumberOfFundingsToRetrieveFromApi,
                                fundingStream);

            var fundingPeriods = (fundingPeriodsCommaSeperated ?? string.Empty).Split(',');
            var fundingScenarios = (scenarioIdsCommaSeperated ?? string.Empty).Split(',');
            var scenarioCount = 0;

            int? originalFundingThroughput = null;
            int? originalProviderFundingThroughput = null;

            try
            {
                LogInfo(logger, originalLogger, "Raising throughput (if required)");

                (originalFundingThroughput, originalProviderFundingThroughput) =
                    await RaiseThroughputIfRequired(cosmosDbConfiguration, cosmosDbClient, desiredThroughPutSize, desiredThroughPutSize);

                LogInfo(logger, originalLogger, "Raising throughput (if required) finished");

                foreach (var fundingPeriod in fundingPeriods)
                {
                    var scenarioId = fundingScenarios[scenarioCount++];
                    var feedUri = feedReaderInputUriModel.OriginalFundingUri.GetFeedFundingUri(
                                                                                    fundingPeriod,
                                                                                    scenarioId,
                                                                                    feedReaderInputUriModel.IsCFSUri);

                    var startDateTime = !string.IsNullOrEmpty(startDateTimeString) ? DateTime.Parse(startDateTimeString) : DateTime.UtcNow;

                    var feedReaderResult = new FeedReaderResultReport
                    {
                        ID = id ?? Guid.NewGuid().ToString(),
                        StartDateTime = startDateTime,
                        FundingUri = feedUri,
                        ProviderFundingUri = feedReaderInputUriModel.ProviderFundingUri,
                        Status = "Started",
                        PartitionKey = startDateTime.ToString("yyyy-MM-dd-HH-mm-ss"),
                        ProgramaticallyChangeThroughput = cosmosDbConfiguration?.ProgramaticallyChangeThroughput ?? false,
                        FundingStreams = fundingStream,
                        FundingPeriods = fundingPeriod,
                        ScenarioIds = scenarioId,
                        SaveCount = 0,
                        Action = "Import",
                        LastUpdatedDateTime = startDateTime,
                        PageSize = environmentVariables.NumberOfFundingsToRetrieveFromApi,
                        Trigger = trigger.ToDisplayText()
                    };

                    var fundingsFeedReader = new FundingsFeedReader(
                                                            feedUri,
                                                            feedReaderInputUriModel.OriginalFundingLookupUri,
                                                            feedReaderInputUriModel.ProviderFundingUri,
                                                            feedReaderInputUriModel.OriginalProviderFundingEnrichmentsUri,
                                                            httpService,
                                                            cosmosDbClient,
                                                            azureSearchServiceManager,
                                                            logger,
                                                            originalLogger,
                                                            environmentVariables.SimultanousCosmosReadWriteCount,
                                                            environmentVariables.TaskBatchSize,
                                                            feedReaderResult,
                                                            maximumPagesToProcess);

                    await fundingsFeedReader.Process(fundingStream, useBookMark)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(logger, originalLogger, ex, ex.Message);
            }
            finally
            {
                LogInfo(logger, originalLogger, "Lowering throughput (if required)");

                await LowerThroughputIfRequired(
                    cosmosDbConfiguration,
                    cosmosDbClient,
                    originalFundingThroughput,
                    originalProviderFundingThroughput);

                LogInfo(logger, originalLogger, "Lowering throughput (if required) finished");
            }
        }

        private static async Task Run(
            ILogger originalLogger,
            string fundingStreamsCommaSeperated = null,
            string fundingPeriodsCommaSeperated = null,
            string scenarioIdsCommaSeperated = null,
            string id = null,
            string startDateTimeString = null,
            bool useBookMark = true,
            RunTrigger trigger = RunTrigger.None)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            originalLogger?.LogInformation($"Version v1.0 Fundings feed reader Azure Function run started: {DateTime.Now}");

            IEnvironmentVariablesModel environmentVariables = new EnvironmentVariablesModel();

            var localSettingValidationResult = environmentVariables.ValidateLocalSettingModel();

            if (!localSettingValidationResult.isValid)
            {
                throw new ArgumentException($"Error: Fundings feed reader has missing configuration setting(s) - {localSettingValidationResult.missingProperties}");
            }

            // The original logger is file system based, so swap to use an app insights logger
            var logger = GetLogger(environmentVariables.Environment, environmentVariables.AppInsightsConnectionString);

            try
            {
                var logMessage = $"Fundings Feed reader function started v1.0, Trigger: {trigger.ToDisplayText()}, Environment: {environmentVariables.Environment}, RunMode: {environmentVariables.RunMode}, PageSize: {environmentVariables.NumberOfFundingsToRetrieveFromApi}";
                LogInfo(logger, originalLogger, logMessage);

                if (environmentVariables.RunMode != "default" && environmentVariables.RunMode != "recovery")
                {
                    LogInfo(logger, originalLogger, "Feed reader processing is turned OFF.To switch ON please change run mode setting to default or recovery");
                    return;
                }

                var cosmosDbClient = new CosmosDocumentClient(
                                                    environmentVariables.CosmosDbEndPoint,
                                                    environmentVariables.CosmosDbKey,
                                                    environmentVariables.CosmosDbName,
                                                    environmentVariables.CosmosFundingGroupCollectionName,
                                                    environmentVariables.CosmosProviderFundingCollectionName,
                                                    environmentVariables.CosmosAuditCollectionName,
                                                    environmentVariables.CosmosConnectionMode,
                                                    logger,
                                                    originalLogger);

                var cosmosDbConfiguration = new CosmosDbConfiguration(
                                                                    cosmosDbClient,
                                                                    logger,
                                                                    environmentVariables.CosmosThroughputWaitTimeSeconds,
                                                                    environmentVariables.CosmosProgramaticallyChangeThroughput);

                // Funding's API authentication
                var authenticationService = environmentVariables.AuthUseAuthentication ?
                                                new AuthenticationService(
                                                                        environmentVariables.AuthAuthority,
                                                                        environmentVariables.AuthTenantId,
                                                                        environmentVariables.AuthClientId,
                                                                        environmentVariables.AuthClientSecret,
                                                                        environmentVariables.AuthAppIdUri) : null;

                // An HttpClient, is actually a shared pool of connections, and will wait until a connection becomes free.
                // So hypothetically the following scenario can happen;

                // - Set max concurrent connections to 1
                // - Make 2 simulate nous requests
                // - First request will be made instantly - imagine that it takes 10 seconds before it completes and releases the connection
                // - Second request has to wait 10 seconds before there is any connections available
                // - That 10 seconds counts towards the timeout (rather then the connection having a maximum 10 second lifespan)
                var handler = new HttpClientHandler
                {
                    MaxConnectionsPerServer = environmentVariables.MaxConnectionsPerServer,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                var httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMinutes(environmentVariables.HttpTimeoutInMinutes)
                };

                var httpService = new HttpService(httpClient, authenticationService, logger, originalLogger);

                if (trigger == RunTrigger.Timer)
                {
                    var isFeedReaderRunning = await IsFeedReaderRunning(cosmosDbClient, environmentVariables.SimultanousCosmosReadWriteCount);
                    if (!isFeedReaderRunning)
                    {
                        var vyfExternalApiService = new VyfExternalApiService(
                                                                environmentVariables.VyfBaseUri,
                                                                environmentVariables.VyfApiKey,
                                                                environmentVariables.AutoPullEndpointUri);
                        fundingStreamsCommaSeperated = await vyfExternalApiService.GetAutoPullFundingStreams(httpClient, logger, originalLogger);
                        if (fundingStreamsCommaSeperated == null)
                        {
                            //No funding streams configured for auto pull, end run.
                            return;
                        }
                    }
                    else
                    {
                        LogInfo(logger, originalLogger, $"Feed reader already processing, try again at next schedule.");
                        return;
                    }
                }
                else
                {
                    if (fundingStreamsCommaSeperated != null)
                    {
                        LogInfo(logger, originalLogger, $"Funding streams to pull: {fundingStreamsCommaSeperated}");
                    }
                }

                var azureSearchServiceClient = new AzureSearchServiceClient(environmentVariables.AsName, environmentVariables.AsAdminKey);
                var azureSearchServiceManager = new AzureSearchServiceManager(azureSearchServiceClient);

                var fundingStreams = fundingStreamsCommaSeperated.Split(',');

                foreach (var fundingStream in fundingStreams)
                {
                    LogInfo(logger, originalLogger, $"Beginning funding feed reader processing for {fundingStream}");

                    await Process(
                        logger,
                        originalLogger,
                        cosmosDbClient,
                        azureSearchServiceManager,
                        cosmosDbConfiguration,
                        httpService,
                        environmentVariables,
                        environmentVariables.CosmosThroughputSize,
                        fundingStream,
                        fundingPeriodsCommaSeperated,
                        scenarioIdsCommaSeperated,
                        id,
                        startDateTimeString,
                        environmentVariables.MaximumPagesToProcess,
                        useBookMark,
                        trigger);

                    LogInfo(logger, originalLogger, $"Funding feed reader processing for {fundingStream} complete");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Fundings feed reader Error: {ex.Message}";
                LogError(logger, originalLogger, ex, errorMessage);

                throw;
            }
            finally
            {
                LogInfo(logger, originalLogger, "Fundings feed reader finished.");
            }
        }

        private static async Task<bool> IsFeedReaderRunning(ICosmosDocumentClient cosmosDbClient, int simultanousCosmosReadWriteCount)
        {
            var concurrentFundingReadWriteLimiter = new SemaphoreSlim(simultanousCosmosReadWriteCount);
            var concurrentCosmosProviderFundingReadSempaphore = new SemaphoreSlim(simultanousCosmosReadWriteCount);
            var existingDataStore = new DataStoreQueries(cosmosDbClient, concurrentFundingReadWriteLimiter, concurrentCosmosProviderFundingReadSempaphore);
            var latestReport = await existingDataStore.GetLatestFeedReaderReport();

            if (latestReport == null)
            {
                return false;
            }

            var result = latestReport.Status == "Started" ? true : false;

            return result;
        }

        private static void LogError(ApplicationLogger.ILogger logger1, ILogger logger2, Exception ex, string errorMessage)
        {
            logger1?.LogException(ex, errorMessage);
            logger2?.LogError(ex, errorMessage);
        }

        private static void LogInfo(ApplicationLogger.ILogger logger1, ILogger logger2, string message)
        {
            logger1?.LogTrace(message);
            logger2?.LogInformation(message);
        }

        private static async Task<(int? setfundingThroughput, int? setProviderFundingThroughput)> RaiseThroughputIfRequired(
            ICosmosDbConfiguration cosmosDbConfiguration,
            ICosmosDocumentClient documentClient,
            int? fundingThroughput,
            int? providerFundingThroughput)
        {
            var setFundingThroughput = ApplyNewCollectionThroughputAsyncFundingCollectionName(
                cosmosDbConfiguration,
                documentClient,
                fundingThroughput);

            var setProviderFundingThroughput = ApplyNewCollectionThroughputAsyncProviderFundingCollectionName(
                cosmosDbConfiguration,
                documentClient,
                providerFundingThroughput);

            return (await setFundingThroughput, await setProviderFundingThroughput);
        }

        private static async Task<int?> ApplyNewCollectionThroughputAsyncFundingCollectionName(
            ICosmosDbConfiguration cosmosDbConfiguration,
            ICosmosDocumentClient documentClient,
            int? fundingThroughput)
        {
            if (cosmosDbConfiguration == null)
            {
                return null;
            }

            return await cosmosDbConfiguration.ApplyNewCollectionThroughputAsync(documentClient.FundingCollectionName, fundingThroughput);
        }

        private static async Task<int?> ApplyNewCollectionThroughputAsyncProviderFundingCollectionName(
            ICosmosDbConfiguration cosmosDbConfiguration,
            ICosmosDocumentClient documentClient,
            int? providerFundingThroughput)
        {
            if (cosmosDbConfiguration == null)
            {
                return null;
            }

            return await cosmosDbConfiguration.ApplyNewCollectionThroughputAsync(documentClient.ProviderFundingCollectionName, providerFundingThroughput);
        }

        private static async Task LowerThroughputIfRequired(
            ICosmosDbConfiguration cosmosDbConfiguration,
            ICosmosDocumentClient documentClient,
            int? originalFundingThroughput,
            int? originalProviderFundingThroughput)
        {
            if (documentClient == null)
            {
                return;
            }

            var dbTasks = new List<Task<int?>>
            {
                cosmosDbConfiguration.ApplyNewCollectionThroughputAsync(documentClient.FundingCollectionName, originalFundingThroughput),
                cosmosDbConfiguration.ApplyNewCollectionThroughputAsync(documentClient.ProviderFundingCollectionName, originalProviderFundingThroughput)
            };

            await Task.WhenAll(dbTasks);
        }

        /// <summary>
        /// Get instance of Application Insight logger.
        /// </summary>
        /// <param name="environment">Environment (e.g. dev).</param>
        /// <param name="appInsightsConnectionString">Application Insight Connection String.</param>
        /// <param name="outputToConsole">Show output on console.</param>
        /// <returns>Instance of ILogger.</returns>
        private static ApplicationLogger.ILogger GetLogger(string environment, string appInsightsConnectionString, bool outputToConsole = false)
        {
            return new ApplicationInsightsLogger(
                appInsightsConnectionString,
                new Dictionary<string, string>
                {
                    { "environment", environment },
                    { "component", "Funding.FeedReader" }
                }, outputToConsole);
        }
    }
}