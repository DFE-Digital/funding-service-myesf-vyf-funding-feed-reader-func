using Clients;
using CorporateSchema.Version4_00;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using MoreLinq;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Data store queries.
    /// </summary>
    public class DataStoreQueries
    {
        private const int MAX_ID_LOOKUPS_IN_QUERY = 50;
        private const int MaxRetryAttempts = 3;
        private readonly TimeSpan pauseBetweenFailures = TimeSpan.FromSeconds(3);

        private readonly ICosmosDocumentClient _client;
        private readonly SemaphoreSlim _concurrentFundingReadLimiter;
        private readonly SemaphoreSlim _concurrentProviderFundingReadLimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStoreQueries"/> class.
        /// </summary>
        /// <param name="documentClient">A document client.</param>
        /// <param name="concurrentFundingReadLimiter">Semaphore for limiting parallel funding queries.</param>
        /// <param name="concurrentProviderFundingReadLimiter">Semaphore for limiting parallel provider funding queries.</param>
        public DataStoreQueries(
            ICosmosDocumentClient documentClient,
            SemaphoreSlim concurrentFundingReadLimiter,
            SemaphoreSlim concurrentProviderFundingReadLimiter)
        {
            _client = documentClient ?? throw new ArgumentNullException("Document client cannot be null");

            _concurrentFundingReadLimiter = concurrentFundingReadLimiter;
            _concurrentProviderFundingReadLimiter = concurrentProviderFundingReadLimiter;
        }

        /// <summary>
        /// Get document count in the provider fundings collection.
        /// </summary>
        /// <returns>Number of provider fundings in collection.</returns>
        public async Task<long> GetFundingGroupCountFromCollection()
        {
            var result = await RetryPolicy().ExecuteAsync(async () =>
            {
                return await _client.CreateDocumentQuery<long>(
                    UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.FundingCollectionName),
                    "SELECT VALUE COUNT(1) FROM c where IS_DEFINED(c.fundingStream) = true",
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }).ToListAsync()
                    .ConfigureAwait(false);
            });

            return result.Count > 0 ? result[0] : 1;
        }

        /// <summary>
        /// Get document count in the provider fundings collection.
        /// </summary>
        /// <returns>Number of provider fundings in collection.</returns>
        public async Task<long> GetProviderFundingsCountFromCollection()
        {
            var result = await RetryPolicy().ExecuteAsync(async () =>
            {
                return await _client.CreateDocumentQuery<long>(
                    UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.ProviderFundingCollectionName),
                    "SELECT VALUE COUNT(1) FROM c where IS_DEFINED(c.fundingStreamCode) = true",
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }).ToListAsync()
                    .ConfigureAwait(false);
            });

            return result.Count > 0 ? result[0] : 1;
        }

        /// <summary>
        /// Get a list of Id's found in the fundings collection.
        /// </summary>
        /// <returns>A list of funding Id's that have been found in the fundings collection.</returns>
        public async Task<List<string>> GetAllPreExistingFundingIds()
        {
            await _concurrentFundingReadLimiter.WaitAsync();

            try
            {
                return await RetryPolicy().ExecuteAsync(async () =>
                {
                    return await _client.CreateDocumentQuery<string>(
                        UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.FundingCollectionName),
                        "SELECT value c.id FROM c",
                        new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                    .ToListAsync()
                    .ConfigureAwait(false);
                });
            }
            finally
            {
                _concurrentFundingReadLimiter.Release();
            }
        }

        /// <summary>
        /// Get a list of Id's found in the provider fundings collection.
        /// </summary>
        /// <returns>A list of funding Id's that have been found in the provider fundings collection.</returns>
        public async Task<List<string>> GetAllPreExistingProviderFundingIds()
        {
            await _concurrentProviderFundingReadLimiter.WaitAsync();

            try
            {
                return await RetryPolicy().ExecuteAsync(async () =>
                {
                    return await _client.CreateDocumentQuery<string>(
                        UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.ProviderFundingCollectionName),
                        "SELECT value c.id FROM c",
                        new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                    .ToListAsync()
                    .ConfigureAwait(false);
                });
            }
            finally
            {
                _concurrentProviderFundingReadLimiter.Release();
            }
        }

        /// <summary>
        /// Get a list of Id's found in the fundings collection.
        /// </summary>
        /// <param name="feedPageResult">The Id's that need to be matched.</param>
        /// <returns>A list of funding Id's that have been found in the fundings collection.</returns>
        public async Task<(FeedResponseModel feedPageResult, List<string> existingFundingIds)> GetExistingFundingMatches(FeedResponseModel feedPageResult)
        {
            var existingFundingIds = new List<string>();

            if (feedPageResult?.AtomEntry?.Any() != true)
            {
                return (feedPageResult, existingFundingIds);
            }

            var fundingIds = feedPageResult.AtomEntry.Select(atom => atom.Content.Funding.Id).ToList();
            var fundingIdBatches = fundingIds.Batch(MAX_ID_LOOKUPS_IN_QUERY);

            foreach (var fundingIdBatch in fundingIdBatches)
            {
                await _concurrentFundingReadLimiter.WaitAsync();

                try
                {
                    var result = await RetryPolicy().ExecuteAsync(async () =>
                    {
                        return await _client.CreateDocumentQuery<string>(
                            UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.FundingCollectionName),
                            GetFundingSql(fundingIdBatch),
                            new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                        .ToListAsync()
                        .ConfigureAwait(false);
                    });

                    existingFundingIds.AddRange(result);
                }
                finally
                {
                    _concurrentFundingReadLimiter.Release();
                }
            }

            return (feedPageResult, existingFundingIds);
        }

        /// <summary>
        /// Get a list of matching Id's found in the provider fundings collection.
        /// </summary>
        /// <param name="providerFundingIdsToLookup">List of Id's to match.</param>
        /// <param name="filteredOutProviderFundingIds">List of ids to return without processing.</param>
        /// <returns>A list of funding Id's that have been found in the provider fundings collection.</returns>
        public async Task<Dictionary<string, List<ParentEnrichment>>> RequestExistingProviderFundingEnrichments(
            IEnumerable<string> providerFundingIdsToLookup,
            IEnumerable<string> filteredOutProviderFundingIds)
        {
            var outputProviderFundingIdsWithEnrichments = new Dictionary<string, List<ParentEnrichment>>();

            foreach (var filteredOutProviderFundingId in filteredOutProviderFundingIds)
            {
                outputProviderFundingIdsWithEnrichments.Add(filteredOutProviderFundingId, new List<ParentEnrichment>());
            }

            var hasAnyProviderFundingIds = providerFundingIdsToLookup?.Any() == true;

            if (!hasAnyProviderFundingIds)
            {
                // Return empty dictionary
                return outputProviderFundingIdsWithEnrichments;
            }

            var providerFundingIdsBatches = providerFundingIdsToLookup.Batch(MAX_ID_LOOKUPS_IN_QUERY);

            foreach (var providerFundingIdsBatch in providerFundingIdsBatches)
            {
                await _concurrentProviderFundingReadLimiter.WaitAsync();

                try
                {
                    var batchResult_preExistingFundingIds = await RetryPolicy().ExecuteAsync(async () =>
                    {
                        return await _client.CreateDocumentQuery<ParentEnrichment>(
                            UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.ProviderFundingCollectionName),
                            GetProviderFundingSql(providerFundingIdsBatch),
                            new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                        .ToListAsync()
                        .ConfigureAwait(false);
                    });

                    foreach (var providerFundingId in providerFundingIdsBatch)
                    {
                        var relevantEnrichments = batchResult_preExistingFundingIds
                            .Where(preExistingProviderFunding => preExistingProviderFunding.ProviderFundingId == providerFundingId)
                            .ToList();

                        var itemFromOutput = outputProviderFundingIdsWithEnrichments.ContainsKey(providerFundingId) ?
                            outputProviderFundingIdsWithEnrichments[providerFundingId] : null;

                        if (itemFromOutput != null)
                        {
                            // We've already output this - will add to it.
                            var newRelevantEnrichments = relevantEnrichments.Where(preExistingEnrichment =>
                                !itemFromOutput.Any(outputEnrichment => preExistingEnrichment.Id == outputEnrichment.Id));

                            itemFromOutput.AddRange(newRelevantEnrichments);
                            continue;
                        }

                        // Note that relevantEnrichments.Count can be zero.
                        outputProviderFundingIdsWithEnrichments.Add(providerFundingId, relevantEnrichments);
                    }
                }
                finally
                {
                    _concurrentProviderFundingReadLimiter.Release();
                }
            }

            return outputProviderFundingIdsWithEnrichments;
        }

        /// <summary>
        /// Upload document to provider funding collection.
        /// </summary>
        /// <param name="providerFundingDocument">provider funding document.</param>
        /// <returns>A Task.</returns>
        public async Task<ResourceResponse<Document>> UploadProviderFundingDocument(Dictionary<string, object> providerFundingDocument)
        {
            await _concurrentProviderFundingReadLimiter.WaitAsync();

            try
            {
                return await RetryPolicy().ExecuteAsync(async () =>
                {
                    return await _client.UpsertDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.ProviderFundingCollectionName),
                        providerFundingDocument);
                });
            }
            finally
            {
                _concurrentProviderFundingReadLimiter.Release();
            }
        }

        /// <summary>
        /// Upload audit document to provider funding collection.
        /// </summary>
        /// <param name="feedReaderResult">The feed reader results.</param>
        /// <returns>A Task.</returns>
        public async Task UpdateAndUploadAudit(FeedReaderResultReport feedReaderResult)
        {
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.AuditCollectionName);

            await RetryPolicy().ExecuteAsync(async () =>
            {
                await _client.UpsertDocumentAsync(documentCollectionUri, feedReaderResult);
            });
        }

        /// <summary>
        /// Gets the book mark data.
        /// </summary>
        /// <param name="fundingStream">The funding stream to collect last successful audit report for.</param>
        /// <returns>Bookmark data.</returns>
        public async Task<BookmarkData> GetBookMarkData(string fundingStream)
        {
            var result = await RetryPolicy().ExecuteAsync(async () => await _client.CreateDocumentQuery<BookmarkData>(
                    UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.AuditCollectionName),
                    GetBookMarkDataSql(fundingStream),
                    new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                .ToListAsync()
                .ConfigureAwait(false));

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Gets the latest feed reader report.
        /// </summary>
        /// <returns>Latest feed reader report.</returns>
        public async Task<FeedReaderResultReport> GetLatestFeedReaderReport()
        {
            var result = await RetryPolicy().ExecuteAsync(async () => await _client.CreateDocumentQuery<BookmarkData>(
                    UriFactory.CreateDocumentCollectionUri(_client.DatabaseName, _client.AuditCollectionName),
                    GetLatestReportSql(),
                    new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                .ToListAsync()
                .ConfigureAwait(false));

            return result.FirstOrDefault();
        }

        private static string GetFundingSql(IEnumerable<string> fundingIds)
        {
            var sqlWhereClause = string.Join(" or ", fundingIds
                .Select(fundingId => $"c.id = \"{SanitiseString(fundingId)}\"")
                .ToArray());

            return $"SELECT VALUE c.id FROM c where {sqlWhereClause}";
        }

        private static string GetBookMarkDataSql(string fundingStream)
        {
            return $"select top 1 * from c where c.action = 'Import' and c.status = 'Successful' and c.fundingStreams = '{fundingStream}' order by c._ts desc";
        }

        private static string GetLatestReportSql()
        {
            return $"select top 1 * from c where c.action = 'Import' order by c._ts desc";
        }

        private static string GetProviderFundingSql(IEnumerable<string> providerFundingIds)
        {
            var sqlWhere = string.Join(" or ", providerFundingIds
                .Select(providerFundingId => $"c.id = \"{Sanitise(providerFundingId)}\"").ToArray());

            return $"SELECT c.id as pfid, p[\"group\"], p.externalPublicationDate, p.id, p.groupingReason, p.statusChangedDate FROM c join p in c.parentInformation where {sqlWhere}";
        }

        /// <summary>
        /// Format the name by removing characters that are not allowed, spaces etc....
        /// </summary>
        /// <param name="originalSearchTerm">The original name of the provider/la.</param>
        /// <returns>A string in the required format.</returns>
        private static string Sanitise(string originalSearchTerm)
        {
            var defaultName = string.Empty;

            if (string.IsNullOrEmpty(originalSearchTerm))
            {
                return defaultName;
            }

            originalSearchTerm = originalSearchTerm.Replace(" ", string.Empty);
            return originalSearchTerm;
        }

        private static string SanitiseString(string input)
        {
            return Sanitise(input.Replace(".", "_DOT_").Replace("-", "_HYPHEN_"))
                .Replace("_DOT_", ".")
                .Replace("_HYPHEN_", "-");
        }

        private AsyncRetryPolicy RetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(MaxRetryAttempts, i => pauseBetweenFailures);
        }
    }
}