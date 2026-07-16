using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Interface for interacting with a Document client.
    /// </summary>
    public interface ICosmosDocumentClient
    {
        /// <summary>
        /// Gets Cosmos Database Name.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Gets provider Funding collection name.
        /// </summary>
        string ProviderFundingCollectionName { get; }

        /// <summary>
        /// Gets funding collection name.
        /// </summary>
        string FundingCollectionName { get; }

        /// <summary>
        /// Gets audit collection name.
        /// </summary>
        string AuditCollectionName { get; }

        /// <summary>
        /// Create document in CosmosDB.
        /// </summary>
        /// <typeparam name="T">The type of instance that the query should resolve to.</typeparam>
        /// <param name="documentCollectionUri">Document collection Uri.</param>
        /// <param name="sqlExpression">CosmosDB Sql query.</param>
        /// <param name="feedOptions">Feed options.</param>
        /// <returns>Queryable object of type T.</returns>
        IQueryable<T> CreateDocumentQuery<T>(Uri documentCollectionUri, string sqlExpression, FeedOptions feedOptions);

        /// <summary>
        /// Upsert a document into CosmosDB.
        /// </summary>
        /// <param name="documentCollectionUri">document collection Uri.</param>
        /// <param name="documentObject">Document object.</param>
        /// <returns>Task with document response object.</returns>
        Task<ResourceResponse<Document>> UpsertDocumentAsync(Uri documentCollectionUri, object documentObject);

        /// <summary>
        /// Change throughput at the database level.
        /// </summary>
        /// <param name="throughputSize">Throughput size.</param>
        /// <returns>True if offer accepted.</returns>
        Task<bool> ChangeThroughputForDatabase(int throughputSize);

        /// <summary>
        /// Get throughput for database.
        /// </summary>
        /// <returns>The size of the current database throughput.</returns>
        Task<int?> GetCurrentThroughputForDatabase();

        /// <summary>
        /// Change throughput size for specified collection.
        /// </summary>
        /// <param name="throughputSize">Throughput size.</param>
        /// <param name="collectionName">Collection name.</param>
        /// <returns>True if offer accepted.</returns>
        Task<bool> ChangeThroughputForCollection(int throughputSize, string collectionName);

        /// <summary>
        /// Get throughput for specified collection.
        /// </summary>
        /// <param name="collectionName">Collection name.</param>
        /// <returns>The size of the current throughput.</returns>
        Task<int?> GetCurrentThroughputForCollection(string collectionName);

        /// <summary>
        /// Create a new collection.
        /// </summary>
        /// <param name="databaseName">Database name.</param>
        /// <param name="collectionName">Collection to remove.</param>
        /// <returns>A Task.</returns>
        Task CreateCollectionAsync(string databaseName, string collectionName);

        /// <summary>
        /// Delete a collection.
        /// </summary>
        /// <param name="dbName">Database name.</param>
        /// <param name="collectionName">Collection to create.</param>
        /// <returns>A Task.</returns>
        Task DeleteCollectionAsync(string dbName, string collectionName);
    }
}