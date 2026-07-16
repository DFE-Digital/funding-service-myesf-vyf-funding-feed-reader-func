using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Interface for managing an Azure Search service.
    /// </summary>
    public interface IAzureSearchServiceManager
    {
        /// <summary>
        /// Get the indexers names with the prefix of the document we are setup for.
        /// </summary>
        /// <returns>A list of index names.</returns>
        Task<IList<string>> GetAllIndexerNames();

        /// <summary>
        /// Runs Azure Search indexer on demand.
        /// </summary>
        /// <param name="indexerName">The name of the indexer.</param>
        /// <returns>As async Task containing the result whether the indexer ran successfully or not.</returns>
        Task<bool> RunIndexer(string indexerName);
    }
}
