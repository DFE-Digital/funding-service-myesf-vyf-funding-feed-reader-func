using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Class for managing an Azure Search service.
    /// </summary>
    public class AzureSearchServiceManager : IAzureSearchServiceManager
    {
        /// <summary>
        /// Gets the instance of the Azure search client.
        /// </summary>
        private readonly IAzureSearchServiceClient _azureSearchServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSearchServiceManager"/> class.
        /// </summary>
        /// <param name="azureSearchServiceClient">The client to use to access Azure Search admin functions.</param>
        public AzureSearchServiceManager(IAzureSearchServiceClient azureSearchServiceClient)
        {
            _azureSearchServiceClient = azureSearchServiceClient;
        }

        #region IAzureSearchServiceManager Implementation

        /// <summary>
        /// Get the indexers names with the prefix of the document we are setup for.
        /// </summary>
        /// <returns>A list of index names.</returns>
        public async Task<IList<string>> GetAllIndexerNames()
        {
            return await _azureSearchServiceClient.GetIndexerNames();
        }

        /// <summary>
        /// Runs Azure Search indexer on demand.
        /// </summary>
        /// <param name="indexerName">The name of the indexer.</param>
        /// <returns>As async Task containing the result whether the indexer ran successfully or not.</returns>
        public async Task<bool> RunIndexer(string indexerName)
        {
            await _azureSearchServiceClient.RunIndexer(indexerName);
            return await _azureSearchServiceClient.WaitForIndexerCompletion(indexerName, TimeSpan.FromMinutes(10));
        }

        #endregion
    }
}
