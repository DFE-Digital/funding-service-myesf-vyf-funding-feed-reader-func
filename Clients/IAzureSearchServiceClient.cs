using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// An interface exposing operations of an Azure Search Service Client.
    /// </summary>
    public interface IAzureSearchServiceClient
    {
        /// <summary>
        /// Get the indexer names with the prefix of the document we are setup for.
        /// </summary>
        /// <returns>A list of indexer names.</returns>
        Task<IList<string>> GetIndexerNames();

        /// <summary>
        /// Runs Azure Search indexer on demand.
        /// </summary>
        /// <param name="indexerName">The name of the indexer.</param>
        /// <returns>As async Task containing the result of CreateAsync(indexer).</returns>
        Task RunIndexer(string indexerName);

        /// <summary>
        /// Wait for the given indexer to finish indexing documents.
        /// </summary>
        /// <param name="indexerName">The name of the indexer to wait for.</param>
        /// <param name="timeout">The maximum amount of time to wait.</param>
        /// <returns>If the indexing operation completed successfully within the allotted time, then true, otherwise false.</returns>
        Task<bool> WaitForIndexerCompletion(string indexerName, TimeSpan timeout);
    }
}
