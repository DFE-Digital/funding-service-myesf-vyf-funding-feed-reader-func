using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// A class exposing operations of an Azure Search Service Client.
    /// </summary>
    public class AzureSearchServiceClient : IAzureSearchServiceClient
    {
        /// <summary>
        /// The service client instance.
        /// </summary>
        private readonly SearchServiceClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSearchServiceClient"/> class.
        /// </summary>
        /// <param name="searchServiceName">The name of the search service to use.</param>
        /// <param name="adminApiKey">An API key that provides access to the admin functions of the search service.</param>
        public AzureSearchServiceClient(string searchServiceName, string adminApiKey)
        {
            _client = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
        }

        #region Implementation of IAzureSearchServiceClient

        /// <summary>
        /// Get the indexer names.
        /// </summary>
        /// <returns>A list of indexer names.</returns>
        public async Task<IList<string>> GetIndexerNames()
        {
            return (await _client.Indexers.ListAsync()).Indexers.Select(indexer => indexer.Name).ToList();
        }

        /// <summary>
        /// Runs Azure Search indexer on demand.
        /// </summary>
        /// <param name="indexerName">The name of the indexer.</param>
        /// <returns>As async Task containing the result of CreateAsync(indexer).</returns>
        public async Task RunIndexer(string indexerName)
        {
            //Check and wait if already the indexer is running
            await WaitForIndexerCompletion(indexerName, TimeSpan.FromMinutes(60));
            try
            {
                await _client.Indexers.RunAsync(indexerName);
            }
            catch (Microsoft.Rest.Azure.CloudException cex)
            {
                //In rare case, the Indexer might be started running (scheduled) between our check and initiated the Run.
                if (cex.Message.Contains("Another indexer invocation is currently in progress", StringComparison.InvariantCultureIgnoreCase))
                {
                    await WaitForIndexerCompletion(indexerName, TimeSpan.FromMinutes(60));
                    await _client.Indexers.RunAsync(indexerName);
                }
            }
        }

        /// <summary>
        /// Wait for the given indexer to finish indexing documents.
        /// </summary>
        /// <param name="indexerName">The name of the indexer to wait for.</param>
        /// <param name="timeout">The maximum amount of time to wait.</param>
        /// <returns>If the indexing operation completed successfully within the allotted time, then true, otherwise false.</returns>
        public async Task<bool> WaitForIndexerCompletion(string indexerName, TimeSpan timeout)
        {
            var isComplete = false;
            var executionTimer = Stopwatch.StartNew();

            while (!isComplete && executionTimer.Elapsed < timeout)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                var indexerInfo = await _client.Indexers.GetStatusAsync(indexerName);
                isComplete = indexerInfo.LastResult?.Status == IndexerExecutionStatus.Success;
            }

            return isComplete;
        }

        #endregion
    }
}
