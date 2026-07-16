using Clients;
using CorporateSchema.Version4_00;
using Domain.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Process fundings and provider fundings from the fundings API.
    /// </summary>
    public class FundingsFeedReader
    {
        #region Constants

        private const string PaymentValue = "Payment";
        private const string LocalAuthorityValue = "LocalAuthority";
        private const string UKPRNValue = "UKPRN";

        #endregion

        private readonly int _maximumPagesToProcess;
        private readonly int _batchSize;

        private readonly DataStoreQueries _existingDataStore;
        private readonly ICosmosDocumentClient _documentClient;
        private readonly IAzureSearchServiceManager _azureSearchServiceManager;
        private readonly ApplicationLogger.ILogger _logger;
        private readonly ILogger _originalLogger;
        private readonly IHttpService _httpClient;
        private readonly FeedReaderResultReport _report;

        private readonly string _feedFundingUri;
        private readonly SemaphoreSlim _concurrentFundingReadWriteLimiter;

        private readonly string _feedProviderFundingUri;
        private readonly SemaphoreSlim _concurrentCosmosProviderFundingReadSempaphore;
        private readonly string _feedProviderFundingEnrichmentsUri;
        private readonly string _feedFundingLookupUri;

        private BookmarkData _bookMarkData = new BookmarkData();

        private bool _cancelled = false;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingsFeedReader" /> class.
        /// </summary>
        /// <param name="feedFundingUri">Funding Uri.</param>
        /// <param name="feedFundingLookupUri">Funding lookup Uri.</param>
        /// <param name="feedProviderFundingUri">Provider funding Uri.</param>
        /// <param name="feedProviderFundingEnrichmentsUri">Provider funding enrichments endpoint Uri.</param>
        /// <param name="httpClient">Funding feed reader client.</param>
        /// <param name="documentClient">Cosmos DB document client e.g. CosmosDB.</param>
        /// <param name="azureSearchServiceManager">The azure search service manager.</param>
        /// <param name="logger">Application logger.</param>
        /// <param name="originalLogger">The original logger.</param>
        /// <param name="simultanousCosmosReadWriteCount">The number of reads and/or writes that can happen simultaneously.</param>
        /// <param name="batchSize">The batch size (how many simulataneous feed pages to ask for).</param>
        /// <param name="feedReaderResult">The feed reader result to keep updating and saving.</param>
        /// <param name="maximumPagesToProcess">Maximum pages to process (limited in some cases).</param>
        public FundingsFeedReader(
            string feedFundingUri,
            string feedFundingLookupUri,
            string feedProviderFundingUri,
            string feedProviderFundingEnrichmentsUri,
            IHttpService httpClient,
            ICosmosDocumentClient documentClient,
            IAzureSearchServiceManager azureSearchServiceManager,
            ApplicationLogger.ILogger logger,
            Microsoft.Extensions.Logging.ILogger originalLogger,
            int simultanousCosmosReadWriteCount,
            int batchSize,
            FeedReaderResultReport feedReaderResult,
            int maximumPagesToProcess)
        {
            _httpClient = httpClient;
            _documentClient = documentClient;
            _azureSearchServiceManager = azureSearchServiceManager;
            _logger = logger;
            _originalLogger = originalLogger;
            _feedFundingUri = feedFundingUri;
            _feedProviderFundingUri = feedProviderFundingUri;
            _feedProviderFundingEnrichmentsUri = feedProviderFundingEnrichmentsUri;
            _maximumPagesToProcess = maximumPagesToProcess;
            _feedFundingLookupUri = feedFundingLookupUri;
            _batchSize = batchSize;

            _concurrentFundingReadWriteLimiter = new SemaphoreSlim(simultanousCosmosReadWriteCount);
            _concurrentCosmosProviderFundingReadSempaphore = new SemaphoreSlim(simultanousCosmosReadWriteCount);

            _existingDataStore = new DataStoreQueries(_documentClient, _concurrentFundingReadWriteLimiter, _concurrentCosmosProviderFundingReadSempaphore);

            _report = feedReaderResult;
        }

        #endregion


        #region Process fundings

        /// <summary>
        /// Process the feed.
        /// </summary>
        /// <param name="fundingStream">Funding stream to process.</param>
        /// <param name="useBookMark">Use BookMark.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Process(string fundingStream, bool useBookMark)
        {
            await _existingDataStore.UpdateAndUploadAudit(_report).ConfigureAwait(false);
            var numberOfAdditionalPages = 0;
            try
            {
                await SetInitialAuditInformation();

                var firstFeedPageResult = await RequestFirstFundingFeedPage().ConfigureAwait(false);

                var previousPageLink = firstFeedPageResult?.Link?.FirstOrDefault(link => link.Rel == "prev-archive");
                numberOfAdditionalPages = GetNumberOfAdditionalPages(previousPageLink);

                LogInfo($"Number of additional pages {numberOfAdditionalPages}");

                var allPreExistingFundingIds = await _existingDataStore.GetAllPreExistingFundingIds();
                LogInfo($"Number of pre existing funding ids {allPreExistingFundingIds.Count}");

                var allPreExistingProviderFundingIds = await _existingDataStore.GetAllPreExistingProviderFundingIds();
                LogInfo($"Number of pre existing provider funding ids {allPreExistingProviderFundingIds.Count}");

                var previousProviderFundingEnhancmentLookups = new List<string>();

                if (useBookMark)
                {
                    _bookMarkData = await _existingDataStore.GetBookMarkData(fundingStream) ?? new BookmarkData();
                    _bookMarkData.ValidBookMark = ValidateBookMarkData();

                    LogInfo(_bookMarkData.ValidBookMark
                        ? $"Valid bookmark. Last read page is {_bookMarkData.LastProcessedPageNumber}. Bookmark functionality Active."
                        : "Bookmark functionality is active but no valid bookmark data found.");
                }
                else
                {
                    LogInfo("Bookmark functionality is not active.");
                }

                var (currentBatchStart, currentBatchEnd) = GetBatchOfPageNumbers(numberOfAdditionalPages, _batchSize - 1);
                LogInfo($"Current batch of numbers {currentBatchStart}, {currentBatchEnd}");

                var isFirstLoop = true;

                do
                {
                    if (!isFirstLoop)
                    {
                        // Shouldn't need to do this, but standard azure functions
                        // only have 1.5GB and this forces the memory cost to drop quicker.
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    // Process funding
                    var funding = await GetFunding(isFirstLoop, currentBatchStart, currentBatchEnd, firstFeedPageResult, allPreExistingFundingIds);
                    var saveFundingRequests = CompletionOfFundingSaveRequests(RequestSaveOfNewToUsFunding(funding));

                    // Start processing provider funding
                    var preExistingProviderFundingEnrichmentRequests = RequestPreExistingProviderFundingEnrichments(
                        funding,
                        allPreExistingProviderFundingIds);

                    await ProcessAndSaveNewToUsOrUpdatedProviderFundings(
                        preExistingProviderFundingEnrichmentRequests,
                        funding,
                        previousProviderFundingEnhancmentLookups,
                        allPreExistingProviderFundingIds,
                        currentBatchStart).ConfigureAwait(false);

                    // Wait for all funding data to finish saving
                    await saveFundingRequests.ConfigureAwait(false);

                    // Carry on loop
                    (currentBatchStart, currentBatchEnd) = GetBatchOfPageNumbers(currentBatchEnd - 1, _batchSize);

                    isFirstLoop = false;
                    firstFeedPageResult = null;
                }
                while (currentBatchStart > 0);

                LogInfo("Import of data finished");

                await RunFundingIndexers();

                LogInfo("Feed read finished");
                _report.Status = _cancelled ? "Cancelled" : "Successful";
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);

                _report.AdditionalInformation = ex.Message;
                _report.Status = "Failed";

                throw;
            }
            finally
            {
                if (_report.Status.Equals("Successful", StringComparison.InvariantCultureIgnoreCase))
                {
                    //if feed reader has not encountered a failed provider funding which needs to be reattempted at logged page number then set to most recent page number
                    if (_report.LastProcessedPageNumber == 0)
                    {
                        _report.LastProcessedPageNumber = numberOfAdditionalPages;
                    }
                }

                _report.FundingsDbCountAfter = await _existingDataStore.GetFundingGroupCountFromCollection()
                    .ConfigureAwait(false);
                _report.ProviderFundingsDbCountAfter = await _existingDataStore.GetProviderFundingsCountFromCollection()
                    .ConfigureAwait(false);
                _report.EndDateTime = DateTime.UtcNow;
                _report.LastUpdatedDateTime = DateTime.UtcNow;

                await _existingDataStore.UpdateAndUploadAudit(_report).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get a list of tasks in the order they complete (much more efficient then Task.WhenAny in a loop).
        /// https://devblogs.microsoft.com/pfxteam/processing-tasks-as-they-complete/.
        /// </summary>
        /// <typeparam name="T">A generic type.</typeparam>
        /// <param name="tasks">An ienumerable of tasks.</param>
        /// <returns>Tasks in the order they complete.</returns>
        private static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();

            var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
            var results = new Task<Task<T>>[buckets.Length];

            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new TaskCompletionSource<Task<T>>();
                results[i] = buckets[i].Task;
            }

            var nextTaskIndex = -1;
            Action<Task<T>> continuation = completed =>
            {
                var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
                bucket.TrySetResult(completed);
            };

            foreach (var inputTask in inputTasks)
            {
                inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            return results;
        }

        /// <summary>
        /// Sanitise the name by removing unallowed characters, spaces etc.... Must match how the name is sanitised in the front end.
        /// </summary>
        /// <param name="originalSearchTerm">The unsanitised name of the provider/la.</param>
        /// <returns>A string in the format we require.</returns>
        private static string MakeSearchableName(string originalSearchTerm)
        {
            // Regular Expressions for processing the search text:
            var spacingCharacters = new Regex(@"[\s\u2212\u2013\u2014\u2010-]+", RegexOptions.Compiled);
            var disallowedCharacters = new Regex(@"[^\w]+", RegexOptions.Compiled);
            var multipleDashes = new Regex(@"_{2,}", RegexOptions.Compiled);

            var defaultName = string.Empty;

            if (string.IsNullOrEmpty(originalSearchTerm))
            {
                return defaultName;
            }

            var cleanSearchTerm = spacingCharacters.Replace(originalSearchTerm, "_");
            cleanSearchTerm = disallowedCharacters.Replace(cleanSearchTerm, string.Empty);
            cleanSearchTerm = multipleDashes.Replace(cleanSearchTerm, "_");
            cleanSearchTerm = cleanSearchTerm.Trim(new[] { '_' });

            return cleanSearchTerm == string.Empty ? defaultName : cleanSearchTerm;
        }

        private bool ValidateBookMarkData()
        {
            return _bookMarkData != null &&
                   _report.FundingUri == _bookMarkData.FundingUri &&
                   _report.PageSize == _bookMarkData.PageSize;
        }

        private async Task<ConcurrentDictionary<string, FundingFeedExtended>> GetFunding(
            bool isFirstLoop,
            int currentBatchStart,
            int currentBatchEnd,
            FeedResponseModel firstFeedPageResult,
            List<string> allPreExistingFundingIds)
        {
            var batchOfFeedPageRequests = RequestBatchOfFeedPages(isFirstLoop, currentBatchStart, currentBatchEnd, firstFeedPageResult);
            LogInfo($"Started requests for pages from  {currentBatchStart} to {currentBatchEnd}");

            var pages = await CombinePagesWithPreExistingInformation(batchOfFeedPageRequests, allPreExistingFundingIds).ConfigureAwait(false);
            return SeperatePagesIntoFunding(pages);
        }

        /// <summary>
        /// Reruns all the indexers with funding in their name after the data load.
        /// </summary>
        /// <returns>The asynchronous task.</returns>
        private async Task RunFundingIndexers()
        {
            LogInfo("Start running funding indexers");

            if (_azureSearchServiceManager == null)
            {
                LogError(new Exception("Azure search service manager is null"), "Azure search service manager is null");
                return;
            }

            var allIndexers = await _azureSearchServiceManager.GetAllIndexerNames();
            LogInfo($"{allIndexers.Count} indexers found");

            foreach (var indexer in allIndexers)
            {
                if (!indexer.Contains("funding"))
                {
                    continue;
                }

                LogInfo($"Running for indexer {indexer}");
                var success = await _azureSearchServiceManager.RunIndexer(indexer);

                if (success)
                {
                    LogInfo($"Indexer : {indexer} ran successfully.");
                }
                else
                {
                    LogInfo($"Indexer : {indexer} re run failed.");
                }
            }

            LogInfo("Finished running funding indexers");
        }

        private (int firstPageNumber, int lastPageNumber) GetBatchOfPageNumbers(int startNumber, int pagesInBatch)
        {
            var firstPage = (_bookMarkData.ValidBookMark && startNumber < _bookMarkData.LastProcessedPageNumber) ? 0 : startNumber;
            var lastPage = (firstPage - pagesInBatch) + 1;
            lastPage = (_bookMarkData.ValidBookMark && lastPage < _bookMarkData.LastProcessedPageNumber) ? _bookMarkData.LastProcessedPageNumber : lastPage;

            if (lastPage < 1)
            {
                lastPage = 1;
            }

            return (firstPage, lastPage);
        }

        private async Task CompletionOfFundingSaveRequests(IEnumerable<Task<FundingFeed>> fundingSaveRequests)
        {
            foreach (var fundingSaveRequest in Interleaved(fundingSaveRequests))
            {
                await (await fundingSaveRequest.ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
        }

        private IEnumerable<Task<FeedResponseModel>> RequestBatchOfFeedPages(bool firstLoop, int firstPageNumber, int lastPageNumber, FeedResponseModel firstFeedPage)
        {
            if (firstLoop)
            {
                yield return Task.FromResult(firstFeedPage);
            }

            for (var feedPageNumber = firstPageNumber; feedPageNumber >= lastPageNumber; feedPageNumber--)
            {
                if (_bookMarkData.ValidBookMark && _bookMarkData.LastProcessedPageNumber > feedPageNumber)
                {
                    continue;
                }

                var feedPageRequest = GetFundingFeedPage(feedPageNumber);
                yield return feedPageRequest;
            }
        }

        private async Task<ResourceResponse<Document>> UploadProviderFundingDocument_AddProviderFundingMessage_AndFreeMemory(Dictionary<string, object> providerFundingFeedLookupResult)
        {
            var returnResponse = await _existingDataStore.UploadProviderFundingDocument(providerFundingFeedLookupResult);

            providerFundingFeedLookupResult.Clear(); // Stop dictionary taking up so much memory - as we won't use this now its saved

            return returnResponse;
        }

        private async Task SetInitialAuditInformation()
        {
            LogInfo($"CFS funding API feed reader processing started");

            _report.FundingsDbCountBefore = await _existingDataStore.GetFundingGroupCountFromCollection()
                .ConfigureAwait(false);
            _report.ProviderFundingsDbCountBefore = await _existingDataStore.GetProviderFundingsCountFromCollection()
                .ConfigureAwait(false);
        }

        private int GetNumberOfAdditionalPages(FeedLink previousPageLink)
        {
            var additionalPages = previousPageLink != null ?
                int.Parse(Regex.Match(previousPageLink.Href.Split('/')[previousPageLink.Href.Split('/').Length - 1], @"\d+").Value) : 0;

            if (additionalPages > _maximumPagesToProcess - 1)
            {
                additionalPages = _maximumPagesToProcess - 1;
            }

            return additionalPages;
        }

        private async Task ProcessAndSaveNewToUsOrUpdatedProviderFundings(
            Task<Dictionary<string, List<ParentEnrichment>>> preExistingProviderFundingEnrichmentRequests,
            ConcurrentDictionary<string, FundingFeedExtended> funding,
            List<string> previousProviderFundingEnhancmentLookups,
            List<string> allPreExistingProviderFundingIds,
            int currentPageNumber)
        {
            const int SIMULTANEOUS_PROVIDER_FUNDINGS_TO_PROCESS = 10;

            var preExistingProviderFundingEnrichments = await preExistingProviderFundingEnrichmentRequests;
            var preExistingProviderFundingEnrichmentsBatches = preExistingProviderFundingEnrichments.Batch(SIMULTANEOUS_PROVIDER_FUNDINGS_TO_PROCESS);

            foreach (var batch in preExistingProviderFundingEnrichmentsBatches)
            {
                var providerFundingLookupRequests = new List<Task<Dictionary<string, object>>>();

                foreach (var perPageProviderFundingInfo in batch)
                {
                    var providerFundingId = perPageProviderFundingInfo.Key;

                    if (previousProviderFundingEnhancmentLookups.Contains(providerFundingId)
                        || allPreExistingProviderFundingIds.Contains(providerFundingId))
                    {
                        continue;
                    }

                    var preExistingEnrichments = perPageProviderFundingInfo.Value;

                    providerFundingLookupRequests.Add(ProcessProviderFunding(
                        preExistingEnrichments,
                        providerFundingId,
                        funding,
                        previousProviderFundingEnhancmentLookups,
                        allPreExistingProviderFundingIds,
                        currentPageNumber));
                }

                var providerFundingSaveRequests = new List<Task<ResourceResponse<Document>>>();

                foreach (var providerFundingLookupRequest in Interleaved(providerFundingLookupRequests))
                {
                    var providerFundingFeedLookupResult = await (await providerFundingLookupRequest.ConfigureAwait(false))
                        .ConfigureAwait(false);

                    // Null means it wasn't new or could not be retrieved from CFS, so didn't need saving
                    if (providerFundingFeedLookupResult == null)
                    {
                        continue;
                    }

                    providerFundingSaveRequests.Add(UploadProviderFundingDocument_AddProviderFundingMessage_AndFreeMemory(providerFundingFeedLookupResult));
                    _report.SaveCount += 1;
                }

                foreach (var providerFundingSaveRequest in Interleaved(providerFundingSaveRequests))
                {
                    // Complete request
                    await (await providerFundingSaveRequest.ConfigureAwait(false)).ConfigureAwait(false);
                }

                await SaveLastUpdatedDateTime().ConfigureAwait(false);
            }

            await SaveLastUpdatedDateTime().ConfigureAwait(false);
        }

        private async Task<Dictionary<string, object>> ProcessProviderFunding(
            List<ParentEnrichment> preExistingEnrichments,
            string providerFundingId,
            ConcurrentDictionary<string, FundingFeedExtended> allFundings,
            List<string> previousProviderFundingEnhancmentLookups,
            List<string> allPreExistingProviderFundingIds,
            int currentPageNumber)
        {
            try
            {
                var providerFundingIsNew = !preExistingEnrichments.Any();

                var firstParentInFeedBatch = allFundings
                    .Select(funding => funding.Value)
                    .FirstOrDefault(funding => funding.FundingFeed.ProviderFundings.Contains(providerFundingId))?
                    .FundingFeed;

                Dictionary<string, object> providerFunding = null;

                if (providerFundingIsNew)
                {
                    providerFunding = await GetProviderFundingFromApi(
                        string.Format(_feedProviderFundingUri, providerFundingId),
                        firstParentInFeedBatch.SchemaVersion,
                        firstParentInFeedBatch.TemplateVersion,
                        firstParentInFeedBatch.StatusChangedDate.ToString());
                }

                var apiEnrichment_FundingIds = await GetProviderFundingEncrichmentsFromApi(providerFundingId);
                previousProviderFundingEnhancmentLookups.Add(providerFundingId);
                allPreExistingProviderFundingIds.Add(providerFundingId);

                var newToUsFundingEnrichments = apiEnrichment_FundingIds
                    .Where(apiEnrichment_FundingId =>
                    {
                        var preExistingEnrichmentsIds = preExistingEnrichments.Select(preExistingEnrichment => preExistingEnrichment.Id);
                        var isPreExisting = preExistingEnrichmentsIds.Contains(apiEnrichment_FundingId);

                        return isPreExisting == false;
                    })
                    .ToList();

                if (newToUsFundingEnrichments.Count == 0)
                {
                    return null;
                }

                foreach (var apiEnrichment_FundingId in apiEnrichment_FundingIds)
                {
                    if (providerFunding == null)
                    {
                        providerFunding = await GetProviderFundingFromApi(
                            string.Format(_feedProviderFundingUri, providerFundingId),
                            firstParentInFeedBatch.SchemaVersion,
                            firstParentInFeedBatch.TemplateVersion,
                            firstParentInFeedBatch.StatusChangedDate.ToString());
                    }

                    var matchInFeedBatch = allFundings.ContainsKey(apiEnrichment_FundingId) ? allFundings[apiEnrichment_FundingId] : null;
                    var funding_extended = matchInFeedBatch;

                    if (funding_extended == null)
                    {
                        var fundingFromFromApi = await GetFundingFromApi(apiEnrichment_FundingId);
                        fundingFromFromApi.Funding.FundingValue = null; // Free up some memory by nulling out the largest object

                        funding_extended = new FundingFeedExtended { ExistsInCosmosDb = false, FundingFeed = fundingFromFromApi.Funding };

                        allFundings.TryAdd(funding_extended.FundingFeed.Id, funding_extended);
                    }

                    if (!providerFunding.ContainsKey("parentInformation"))
                    {
                        providerFunding.Add("parentInformation", new List<ParentInformation>());
                    }

                    var parentInfo = (List<ParentInformation>)providerFunding["parentInformation"];

                    var fundingEntry = funding_extended.FundingFeed;
                    parentInfo.Add(new ParentInformation
                    {
                        ExternalPublicationDate = fundingEntry.ExternalPublicationDate,
                        GroupingReason = fundingEntry.GroupingReason,
                        Group = fundingEntry.OrganisationGroup,
                        Id = fundingEntry.Id,
                        StatusChangedDate = fundingEntry.StatusChangedDate
                    });
                }

                providerFunding.Add("createdDate", DateTime.UtcNow);

                return providerFunding;
            }
            catch
            {
                _report.LastProcessedPageNumber = currentPageNumber;
                return null;
            }
        }

        private IEnumerable<Task<FundingFeed>> RequestSaveOfNewToUsFunding(ConcurrentDictionary<string, FundingFeedExtended> fundings)
        {
            foreach (var fundingExtended in fundings.Values)
            {
                var isPreExisting = fundingExtended?.ExistsInCosmosDb != false;
                var funding = fundingExtended?.FundingFeed;

                if (isPreExisting)
                {
                    if (funding != null)
                    {
                        funding.FundingValue = null; // This object is very large, so null it out as we dont need it after this
                    }

                    continue;
                }

                var fundingIdParts = funding.Id.Split('-');

                if (fundingIdParts.Length < 6)
                {
                    throw new FormatException($"Funding {funding.Id} not in expected format");
                }

                var partitionKey = fundingIdParts[5];
                funding.PartitionKey = partitionKey;

                var isNewNameRequired = funding.OrganisationGroup.GroupTypeCode == LocalAuthorityValue
                    && funding.GroupingReason == PaymentValue
                    && !funding.OrganisationGroup.Name.Any(char.IsLower)
                    && funding.ProviderFundings.Any();

                if (isNewNameRequired)
                {
                    var firstProviderFundingId = funding.ProviderFundings.First();

                    yield return GetLocalAuthorityNameFromProviderFundingsApiAndSave(funding, firstProviderFundingId);
                    continue;
                }

                yield return RequestSaveFunding_AddMessages_ThenFreeUpMemory(funding);
            }
        }

        private async Task<FundingFeed> RequestSaveFunding_AddMessages_ThenFreeUpMemory(FundingFeed fundingFeed)
        {
            fundingFeed.OrganisationGroup.SearchableName = MakeSearchableName(fundingFeed.OrganisationGroup.Name);
            fundingFeed.CreatedDate = DateTime.UtcNow;

            await _concurrentFundingReadWriteLimiter.WaitAsync();

            try
            {
                await _documentClient.UpsertDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri(_documentClient.DatabaseName, _documentClient.FundingCollectionName),
                    fundingFeed);

                _report.SaveCount += 1;
            }
            finally
            {
                _concurrentFundingReadWriteLimiter.Release();
            }

            fundingFeed.FundingValue = null; // Free up some memory by nulling out the largest object
            return fundingFeed;
        }

        private ConcurrentDictionary<string, FundingFeedExtended> SeperatePagesIntoFunding(Dictionary<FeedResponseModel, List<string>> pagesWithAddedPreExistingInformation)
        {
            var outputFundingsDictionary = new Dictionary<string, FundingFeedExtended>();

            foreach (var extendedPage in pagesWithAddedPreExistingInformation)
            {
                var feedPage = extendedPage.Key;
                var preExistingFundingIds = extendedPage.Value;

                foreach (var atomEtry in feedPage.AtomEntry)
                {
                    var funding = atomEtry.Content.Funding;
                    funding.SchemaVersion = atomEtry.Content.SchemaVersion;
                    var fundingId = funding.Id;

                    var isPreExisting = preExistingFundingIds.Contains(fundingId);
                    var outputAlready = outputFundingsDictionary.ContainsKey(fundingId);

                    // We've already seen it in this method - so extend what we've got
                    if (outputAlready)
                    {
                        var outputFunding = outputFundingsDictionary[fundingId];

                        if (isPreExisting)
                        {
                            // This takes up a lot of memory - and we don't need it in this circumstance
                            outputFunding.FundingFeed.FundingValue = null;
                        }

                        if (outputFunding.ExistsInCosmosDb)
                        {
                            continue;
                        }

                        outputFunding.ExistsInCosmosDb = isPreExisting;
                    }
                    else
                    {
                        if (isPreExisting)
                        {
                            // This takes up a lot of memory - and we don't need it in this circumstance
                            funding.FundingValue = null;
                        }

                        outputFundingsDictionary.Add(fundingId, new FundingFeedExtended
                        {
                            FundingFeed = funding,
                            ExistsInCosmosDb = isPreExisting
                        });
                    }
                }
            }

            return new ConcurrentDictionary<string, FundingFeedExtended>(
                outputFundingsDictionary.Values.ToDictionary(value => value.FundingFeed.Id, value => value));
        }

        private Task<Dictionary<string, List<ParentEnrichment>>> RequestPreExistingProviderFundingEnrichments(
            ConcurrentDictionary<string, FundingFeedExtended> funding,
            List<string> allPreExistingProviderFundingIds)
        {
            var fundingEnumerable = funding.Values.Select(atomEntry => atomEntry.FundingFeed);
            var providerFundingIds = fundingEnumerable.SelectMany(funding => funding.ProviderFundings).Distinct();

            var existingButUpdatedProviderFundingIds = providerFundingIds
                .Where(providerFundingId => allPreExistingProviderFundingIds.Contains(providerFundingId)) // We have it already
                .Where(providerFundingId => ProviderFunding_AnyParentFundingIsNotPreExisting(funding, providerFundingId)) // And it has a parent we haven't seen before
                .ToList();

            var filteredOutProviderFundingIds = providerFundingIds
                .Where(providerFundingId => !existingButUpdatedProviderFundingIds.Contains(providerFundingId));

            return _existingDataStore.RequestExistingProviderFundingEnrichments(existingButUpdatedProviderFundingIds, filteredOutProviderFundingIds);
        }

        private bool ProviderFunding_AnyParentFundingIsNotPreExisting(ConcurrentDictionary<string, FundingFeedExtended> funding, string providerFundingId)
        {
            foreach (var fundingEntry in funding.Values)
            {
                if (fundingEntry.FundingFeed.ProviderFundings.Contains(providerFundingId))
                {
                    if (!fundingEntry.ExistsInCosmosDb)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<Dictionary<FeedResponseModel, List<string>>> CombinePagesWithPreExistingInformation(
            IEnumerable<Task<FeedResponseModel>> feedPageRequests,
            List<string> allPreExistingFundingIds)
        {
            await SaveLastUpdatedDateTime().ConfigureAwait(false);

            var pagesWithAddedPreExistingInformation = new Dictionary<FeedResponseModel, List<string>>();

            var auditOnly_numberOfFundings = 0;
            var auditOnly_allProviderFundingIds = new List<string>();

            foreach (var feedPageRequest in Interleaved(feedPageRequests))
            {
                var feedPage = await (await feedPageRequest.ConfigureAwait(false)).ConfigureAwait(false);

                if (feedPage?.AtomEntry == null)
                {
                    continue;
                }

                INJECT_POINT_FOR_MOCK_SetCancelledStatusIfRelevant(feedPage);
                var preExistingMatchedFundingIdsForPage = new List<string>();

                foreach (var individualFundingAtomEntry in feedPage.AtomEntry)
                {
                    var funding = individualFundingAtomEntry?.Content?.Funding;
                    var isPreExisting = allPreExistingFundingIds.Contains(funding.Id);

                    if (isPreExisting)
                    {
                        preExistingMatchedFundingIdsForPage.Add(funding.Id);
                        funding.FundingValue = null; // Free up some memory by nulling out the largest object
                    }

                    auditOnly_allProviderFundingIds.AddRange(funding.ProviderFundings);
                    auditOnly_numberOfFundings += 1;
                }

                pagesWithAddedPreExistingInformation.Add(feedPage, preExistingMatchedFundingIdsForPage);
            }

            LogInfo($"Number of fundings = {auditOnly_numberOfFundings}");
            _report.FundingsCountInApi = auditOnly_numberOfFundings;

            LogInfo($"Total number of provider fundings = {auditOnly_allProviderFundingIds.Count}");
            _report.ProviderFundingsInApiCountIncludingDuplicates = auditOnly_allProviderFundingIds.Count;

            var distinctProviderFundingCount = auditOnly_allProviderFundingIds.Distinct().Count();
            LogInfo($"Number of distinct provider fundings = {distinctProviderFundingCount}");
            _report.ProviderFundingsInApiCountDistinct = distinctProviderFundingCount;

            await SaveLastUpdatedDateTime().ConfigureAwait(false);

            return pagesWithAddedPreExistingInformation;
        }

        private void INJECT_POINT_FOR_MOCK_SetCancelledStatusIfRelevant(FeedResponseModel feedPageResult)
        {
            _cancelled = feedPageResult.Cancelled;
        }

        private async Task SaveLastUpdatedDateTime()
        {
            var hasBeenMoreThen30Seconds = (DateTime.Now - _report.LastUpdatedDateTime).TotalSeconds > 30;

            if (!hasBeenMoreThen30Seconds)
            {
                return;
            }

            _report.LastUpdatedDateTime = DateTime.Now;
            await _existingDataStore.UpdateAndUploadAudit(_report);
        }

        #endregion


        #region Helper methods

        private void LogError(Exception ex, string errorMessage)
        {
            _logger?.LogException(ex, errorMessage);
            _originalLogger?.LogError(ex, errorMessage);
        }

        private void LogInfo(string message)
        {
            _logger?.LogTrace(message);
            _originalLogger?.LogInformation(message);
        }

        /// <summary>
        /// Get local authority name from the provider fundings API.
        /// </summary>
        /// <param name="feed">The feed to lookk at.</param>
        /// <param name="providerFundingId">Provider Funding Id to retrieve from Provider fundings API.</param>
        /// <returns>The local authority name.</returns>
        private async Task<FundingFeed> GetLocalAuthorityNameFromProviderFundingsApiAndSave(FundingFeed feed, string providerFundingId)
        {
            var providerUrl = string.Format(_feedProviderFundingUri, providerFundingId);

            var providerFundingDocument = await GetProviderFundingFromApi(
                providerUrl,
                feed.SchemaVersion,
                feed.TemplateVersion,
                feed.StatusChangedDate.ToString());

            var provider = ((JObject)providerFundingDocument["provider"]).ToObject<Provider>();
            feed.OrganisationGroup.Name = provider.ProviderDetails.LocalAuthorityName;

            await RequestSaveFunding_AddMessages_ThenFreeUpMemory(feed);
            return feed;
        }

        #endregion


        #region Fundings API

        private async Task<FeedResponseModel> RequestFirstFundingFeedPage()
        {
            return await GetFundingFeedPage();
        }

        /// <summary>
        /// Get feed page from the funding API.
        /// </summary>
        /// <param name="pageNumber">The optional page number to look at.</param>
        /// <returns>A feed page containing a list of fundings from the funding API.</returns>
        private async Task<FeedResponseModel> GetFundingFeedPage(int? pageNumber = null)
        {
            var pageNumberWithSlash = pageNumber != null ? $"/{pageNumber}" : null;
            var url = string.Format(_feedFundingUri, pageNumberWithSlash);
            FeedResponseModel result = null;
            var responseStr = string.Empty;
            var attempt = 1;

            do
            {
                try
                {
                    responseStr = await _httpClient.GetAsync(url).ConfigureAwait(false);
                    result = JsonConvert.DeserializeObject<FeedResponseModel>(responseStr);
                    break;
                }
                catch (Exception exception)
                {
                    LogInfo(
                        $"Feed Page could not be deserialised - on attempt {attempt}, for {url} with error message '{exception.Message}' Response String :- {responseStr}");

                    if (attempt == 3)
                    {
                        throw;
                    }

                    attempt++;
                }
            }
            while (attempt < 4);

            if (result == null)
            {
                throw new FormatException($"Funding Feed Page data for {url} not in expected format");
            }

            return result;
        }

        /// <summary>
        /// Get feed page from the funding API.
        /// </summary>
        /// <param name="fundingId">The optional page number to look at.</param>
        /// <returns>A feed page containing a list of fundings from the funding API.</returns>
        private async Task<FeedBaseModel> GetFundingFromApi(string fundingId)
        {
            var url = string.Format(_feedFundingLookupUri, fundingId);

            try
            {
                var responseStr = await _httpClient.GetAsync(url).ConfigureAwait(false);
                var feedBaseModel = JsonConvert.DeserializeObject<FeedBaseModel>(responseStr);
                feedBaseModel.Funding.SchemaVersion = feedBaseModel.SchemaVersion;

                return feedBaseModel;
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get provider funding parent enrichments.
        /// </summary>
        /// <param name="providerFundingId">The optional page number to look at.</param>
        /// <returns>A list of fundings from the fundings API.</returns>
        private async Task<List<string>> GetProviderFundingEncrichmentsFromApi(string providerFundingId)
        {
            var url = string.Format(_feedProviderFundingEnrichmentsUri, providerFundingId);

            try
            {
                var responseStr = await _httpClient.GetAsync(url).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<IEnumerable<FundingIdObject>>(responseStr)
                    .Select(fundingIdObject => fundingIdObject.FundingId)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get provider funding from the fundings API.
        /// </summary>
        /// <param name="providerFundingUri">The URI of the provider funding.</param>
        /// <param name="schemaVersion">The schema version to enrich provider funding with.</param>
        /// <param name="templateVersion">The template version to enrich provider funding with.</param>
        /// <param name="statusChangedDate">The status changed date to enrich provider funding with.</param>
        /// <returns>Returns a provider funding.</returns>
        private async Task<Dictionary<string, object>> GetProviderFundingFromApi(
            string providerFundingUri,
            string schemaVersion,
            string templateVersion,
            string statusChangedDate)
        {
            try
            {
                var responseStr = await _httpClient.GetAsync(providerFundingUri)
                    .ConfigureAwait(false);

                var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseStr);
                var provider = ((JObject)responseDict["provider"]).ToObject<Provider>();

                var ukprn = provider.OtherIdentifiers.First(identifier => identifier.Type == UKPRNValue).Value;

                responseDict.Add("partitionKey", ukprn);
                responseDict.Add("schemaVersion", schemaVersion);
                responseDict.Add("templateVersion", templateVersion);
                responseDict.Add("statusChangedDate", statusChangedDate);

                return responseDict;
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                throw;
            }
        }

        #endregion

    }
}