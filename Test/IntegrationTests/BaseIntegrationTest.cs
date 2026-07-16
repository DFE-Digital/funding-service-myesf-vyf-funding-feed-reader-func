using Clients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test.IntegrationTests
{
    public class BaseIntegrationTest
    {
        /// <summary>
        /// Gets or sets databaseName.
        /// </summary>
        public string DatabaseName { get; set; } = "funding";

        /// <summary>
        /// Gets or sets fundingCollectionName.
        /// </summary>
        public string FundingCollectionName { get; set; } = "fundingtest";

        /// <summary>
        /// Gets or sets provider funding collection name.
        /// </summary>
        public string ProviderFundingCollectionName { get; set; } = "providerfundingtest";

        /// <summary>
        /// Gets or sets cosmos document client.
        /// </summary>
        public CosmosDocumentClient CosmosDocumentClient { get; set; }

        /// <summary>
        /// Create test CosmosDB collections.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task CreateCollectionsAsync()
        {
            var dbTasks = new List<Task>
            {
                CosmosDocumentClient.CreateCollectionAsync(DatabaseName, FundingCollectionName),
                CosmosDocumentClient.CreateCollectionAsync(DatabaseName, ProviderFundingCollectionName)
            };

            await Task.WhenAll(dbTasks.ToArray());
        }

        /// <summary>
        /// Delete test CosmosDB collections.
        /// </summary>
        /// <param name="cosmosDocumentClient">CosmosDB client.</param>
        /// <returns>Task.</returns>
        public async Task DeleteCollectionsAsync(CosmosDocumentClient cosmosDocumentClient)
        {
            try
            {
                var dbTasks = new List<Task>
            {
                cosmosDocumentClient.DeleteCollectionAsync(DatabaseName, FundingCollectionName),
                cosmosDocumentClient.DeleteCollectionAsync(DatabaseName, ProviderFundingCollectionName)
            };

                await Task.WhenAll(dbTasks.ToArray());
            }
            catch
            {
                // Perhaps they didn't exist already
            }
        }
    }
}
