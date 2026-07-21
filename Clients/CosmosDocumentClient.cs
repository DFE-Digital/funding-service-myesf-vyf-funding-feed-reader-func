using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Implementation of a CosmosDB document client.
    /// </summary>
    public class CosmosDocumentClient : ICosmosDocumentClient
    {
        private const int MaxRetryAttempts = 3;
        private readonly TimeSpan pauseBetweenFailures = TimeSpan.FromSeconds(10);
        private readonly DocumentClient _documentClient;
        private readonly ApplicationLogger.ILogger _logger;
        private readonly ILogger _originalLogger;

        /// <summary>
        /// Gets the Cosmos Database Name.
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets provider Funding collection name.
        /// </summary>
        public string ProviderFundingCollectionName { get; private set; }

        /// <summary>
        /// Gets funding collection name.
        /// </summary>
        public string FundingCollectionName { get; private set; }

        /// <summary>
        /// Gets audit collection name.
        /// </summary>
        public string AuditCollectionName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDocumentClient" /> class.
        /// </summary>
        /// <param name="endpointUri">CosmosDB Uri.</param>
        /// <param name="key">CosmosDB key.</param>
        /// <param name="databaseName">Cosmos Database name.</param>
        /// <param name="fundingCollectionName">Funding collection name.</param>
        /// <param name="providerFundingCollectionName">Provider funding collection name.</param>
        /// <param name="auditCollectionName">Audit collection name.</param>
        /// <param name="connectionMode">Cosmos Connection Mode.</param>
        /// <param name="logger">Application logger.</param>
        /// <param name="originalLogger">The original logger.</param>
        public CosmosDocumentClient(
            string endpointUri,
            string key,
            string databaseName,
            string fundingCollectionName,
            string providerFundingCollectionName,
            string auditCollectionName,
            string connectionMode,
            ApplicationLogger.ILogger logger,
            ILogger originalLogger)
        {
            var cdConnectionMode = connectionMode.Equals("Gateway", StringComparison.InvariantCultureIgnoreCase) ?
                                                                        ConnectionMode.Gateway : ConnectionMode.Direct;
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = cdConnectionMode,
                ConnectionProtocol = Protocol.Tcp,
                RetryOptions = new RetryOptions
                {
                    MaxRetryAttemptsOnThrottledRequests = 50,
                    MaxRetryWaitTimeInSeconds = 60
                },
            };

            if (string.IsNullOrEmpty(endpointUri))
            {
                throw new ArgumentNullException("Cosmos endpoint cannot be null");
            }

            _documentClient = new DocumentClient(new Uri(endpointUri), key, connectionPolicy);
            _logger = logger;
            _originalLogger = originalLogger;

            DatabaseName = databaseName;
            FundingCollectionName = fundingCollectionName;
            ProviderFundingCollectionName = providerFundingCollectionName;
            AuditCollectionName = auditCollectionName;
        }

        /// <summary>
        /// Create document in CosmosDB.
        /// </summary>
        /// <typeparam name="T">The type of instance that the query should resolve to.</typeparam>
        /// <param name="documentCollectionOrDatabaseUri">Document collection Uri.</param>
        /// <param name="sqlExpression">CosmosDB Sql query.</param>
        /// <param name="feedOptions">Feed options.</param>
        /// <returns>The new query.</returns>
        public IQueryable<T> CreateDocumentQuery<T>(Uri documentCollectionOrDatabaseUri, string sqlExpression, FeedOptions feedOptions)
        {
            return _documentClient.CreateDocumentQuery<T>(documentCollectionOrDatabaseUri, sqlExpression, feedOptions);
        }

        /// <summary>
        /// Upsert a document into CosmosDB.
        /// </summary>
        /// <param name="documentCollectionUri">document collection Uri.</param>
        /// <param name="funding">Document object.</param>
        /// <returns>Task with document response object.</returns>
        public async Task<ResourceResponse<Document>> UpsertDocumentAsync(Uri documentCollectionUri, object funding)
        {
            var result = await RetryPolicy().ExecuteAsync(async () =>
            {
                return await _documentClient.UpsertDocumentAsync(documentCollectionUri, funding);
            });

            _logger?.LogTrace($"Write into {documentCollectionUri} took  {result.RequestCharge}RUs");
            _originalLogger?.LogInformation($"Write into {documentCollectionUri} took  {result.RequestCharge}RUs");

            return result;
        }

        #region CosmosDb Configuration

        /// <summary>
        /// Change throughput at the database level.
        /// </summary>
        /// <param name="throughputSize">Throughput size.</param>
        /// <returns>True if offer accepted.</returns>
        public async Task<bool> ChangeThroughputForDatabase(int throughputSize)
        {
            var offer = await GetCurrentThroughputAtDbLevelOffer();

            if (offer.Content.OfferThroughput == throughputSize)
            {
                return false;
            }

            var updatedOffer = new OfferV2(offer, offerThroughput: throughputSize, offerEnableRUPerMinuteThroughput: true);

            await _documentClient.ReplaceOfferAsync(updatedOffer);
            return true;
        }

        /// <summary>
        /// Get throughput for database.
        /// </summary>
        /// <returns>The size of the current database throughput.</returns>
        public async Task<int?> GetCurrentThroughputForDatabase()
        {
            var offer = await GetCurrentThroughputAtDbLevelOffer();
            return offer?.Content?.OfferThroughput;
        }

        /// <summary>
        /// Get throughput for specified collection.
        /// </summary>
        /// <param name="collectionName">Collection name.</param>
        /// <returns>The size of the current throughput.</returns>
        public async Task<int?> GetCurrentThroughputForCollection(string collectionName)
        {
            var offer = await GetCurrentThroughputOfferForCollection(collectionName);
            return offer?.Content?.OfferThroughput;
        }

        /// <summary>
        /// Change throughput size for specified collection.
        /// </summary>
        /// <param name="throughputSize">Throughput size.</param>
        /// <param name="collectionName">Collection name.</param>
        /// <returns>True if new size was successfully applied.</returns>
        public async Task<bool> ChangeThroughputForCollection(int throughputSize, string collectionName)
        {
            var offer = await GetCurrentThroughputOfferForCollection(collectionName);

            if (offer.Content.OfferThroughput == throughputSize)
            {
                return false;
            }

            var updatedOffer = new OfferV2(offer, offerThroughput: throughputSize, offerEnableRUPerMinuteThroughput: true);

            await _documentClient.ReplaceOfferAsync(updatedOffer);
            return true;
        }

        /// <summary>
        /// Create a new collection.
        /// </summary>
        /// <param name="databaseName">Database name.</param>
        /// <param name="collectionName">Collection to remove.</param>
        /// <returns>A Task.</returns>
        public async Task CreateCollectionAsync(string databaseName, string collectionName)
        {
            var throughput = 4000;

            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                new DocumentCollection
                {
                    Id = collectionName,
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new System.Collections.ObjectModel.Collection<string> { "/partitionKey" }
                    },
                    IndexingPolicy = new IndexingPolicy
                    {
                        Automatic = true,
                        IndexingMode = IndexingMode.Consistent,
                        IncludedPaths = new System.Collections.ObjectModel.Collection<IncludedPath>
                        {
                            new IncludedPath
                            {
                                Path = "/provider/name/*"
                            },
                            new IncludedPath
                            {
                                Path = "/provider/otherIdentifiers/[]/*"
                            },
                            new IncludedPath
                            {
                                Path = "/organisationGroup/name/*"
                            }
                        },
                        ExcludedPaths = new System.Collections.ObjectModel.Collection<ExcludedPath>
                        {
                            new ExcludedPath
                            {
                                Path = "/*"
                            }
                        }
                    }
                },
                new RequestOptions { OfferThroughput = throughput });
        }

        /// <summary>
        /// Delete a collection.
        /// </summary>
        /// <param name="dbName">Database name.</param>
        /// <param name="collectionName">Collection to create.</param>
        /// <returns>A Task.</returns>
        public async Task DeleteCollectionAsync(string dbName, string collectionName)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);
            await _documentClient.DeleteDocumentCollectionAsync(uri);
        }

        /// <summary>
        /// Get throughput size at database level.
        /// </summary>
        /// <returns>Throughput offering for database.</returns>
        private async Task<OfferV2> GetCurrentThroughputAtDbLevelOffer()
        {
            var databaseUri = UriFactory.CreateDatabaseUri(DatabaseName);

            var db = await _documentClient.ReadDatabaseAsync(databaseUri);
            var results = await _documentClient.CreateOfferQuery()
                .Where(o => o.ResourceLink == db.Resource.SelfLink)
                .AsDocumentQuery()
                .ExecuteNextAsync<OfferV2>();
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Get throughput for specified collection.
        /// </summary>
        /// <param name="collectionName">Collection name.</param>
        /// <returns>The current throughput offer.</returns>
        private async Task<OfferV2> GetCurrentThroughputOfferForCollection(string collectionName)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
            var response = await _documentClient.ReadDocumentCollectionAsync(collectionUri);
            var collection = response.Resource;

            var results = await _documentClient.CreateOfferQuery()
                .Where(o => o.ResourceLink == collection.SelfLink)
                .AsDocumentQuery()
                .ExecuteNextAsync<OfferV2>();

            return results.FirstOrDefault();
        }

        #endregion

        private AsyncRetryPolicy RetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(MaxRetryAttempts, i => pauseBetweenFailures);
        }
    }
}