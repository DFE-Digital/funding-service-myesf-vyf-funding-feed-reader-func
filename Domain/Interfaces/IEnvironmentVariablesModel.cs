namespace Domain.Interfaces
{
    /// <summary>
    /// An Interface to represent Environmental Variables (local.settings.json for local development) to execute the Function App.
    /// </summary>
    public interface IEnvironmentVariablesModel
    {
        // Application Insights

        /// <summary>
        /// Gets the Environment details of the current Instance to shown in Application Insights.
        /// </summary>
        string Environment { get; }

        /// <summary>
        /// Gets the App Insights Connection String.
        /// </summary>
        string AppInsightsConnectionString { get; }

        // Azure Search

        /// <summary>
        /// Gets the Azure Search Name.
        /// </summary>
        string AsName { get; }

        /// <summary>
        /// Gets the Azure Search Admin Key.
        /// </summary>
        string AsAdminKey { get; }

        // Cosmos DB

        /// <summary>
        /// Gets the Cosmos Audit Collection Name.
        /// </summary>
        string CosmosAuditCollectionName { get; }

        /// <summary>
        /// Gets the Cosmos Database Endpoint.
        /// </summary>
        string CosmosDbEndPoint { get; }

        /// <summary>
        /// Gets the Cosmos Database Key.
        /// </summary>
        string CosmosDbKey { get; }

        /// <summary>
        /// Gets the Cosmos Database Name.
        /// </summary>
        string CosmosDbName { get; }

        /// <summary>
        /// Gets the Cosmos Funding Group Collection Name.
        /// </summary>
        string CosmosFundingGroupCollectionName { get; }

        /// <summary>
        /// Gets a value indicating whether Cosmos throughput can change programmatically.
        /// </summary>
        bool CosmosProgramaticallyChangeThroughput { get; }

        /// <summary>
        /// Gets the Cosmos Provider Funding Collection Name.
        /// </summary>
        string CosmosProviderFundingCollectionName { get; }

        /// <summary>
        /// Gets the Cosmos Throughput Size.
        /// </summary>
        int CosmosThroughputSize { get; }

        /// <summary>
        /// Gets the Cosmos Throuphput Wait Time in Seconds.
        /// </summary>
        int CosmosThroughputWaitTimeSeconds { get; }

        /// <summary>
        /// Gets the Simultanous Cosmos Read/Write count.
        /// </summary>
        int SimultanousCosmosReadWriteCount { get; }

        /// <summary>
        /// Gets the Cosmos Provider Funding Collection Name.
        /// </summary>
        string CosmosConnectionMode { get; }

        // Funding's API authentication

        /// <summary>
        /// Gets the Authentication App ID URI.
        /// </summary>
        string AuthAppIdUri { get; }

        /// <summary>
        /// Gets the Authentication Authority.
        /// </summary>
        string AuthAuthority { get; }

        /// <summary>
        /// Gets the Authentication Client ID.
        /// </summary>
        string AuthClientId { get; }

        /// <summary>
        /// Gets the Authentication Client Secret.
        /// </summary>
        string AuthClientSecret { get; }

        /// <summary>
        /// Gets the Authentication Tenant ID.
        /// </summary>
        string AuthTenantId { get; }

        /// <summary>
        /// Gets a value indicating whether to use Authentication.
        /// </summary>
        bool AuthUseAuthentication { get; }

        // VYF API

        /// <summary>
        /// Gets the VYF Api Key.
        /// </summary>
        string VyfApiKey { get; }

        /// <summary>
        /// Gets the VYF Base Uri.
        /// </summary>
        string VyfBaseUri { get; }

        /// <summary>
        /// Gets the Auto Pull Endpoint URI.
        /// </summary>
        string AutoPullEndpointUri { get; }

        // Others

        /// <summary>
        /// Gets the Fundings API URI.
        /// </summary>
        string FundingsApiUri { get; }

        /// <summary>
        /// Gets the HTTP Timeout in Minutes.
        /// </summary>
        int HttpTimeoutInMinutes { get; }

        /// <summary>
        /// Gets the Max connections per Server.
        /// </summary>
        int MaxConnectionsPerServer { get; }

        /// <summary>
        /// Gets the Maxium Pages to Process.
        /// </summary>
        int MaximumPagesToProcess { get; }

        /// <summary>
        /// Gets the Number of Fundings to Retrieve from Api.
        /// </summary>
        int NumberOfFundingsToRetrieveFromApi { get; }

        /// <summary>
        /// Gets the Run Mode.
        /// </summary>
        string RunMode { get; }

        /// <summary>
        /// Gets the Size of the Parallel Task per Batch.
        /// </summary>
        int TaskBatchSize { get; }
    }
}