using Domain.Interfaces;
using Pds_azurefunction_fundingfeedreader.Helpers;

namespace Pds_azurefunction_fundingfeedreader.Models
{
    /// <summary>
    /// A Class to represent Environmental Variables (local.settings.json for local development) to to execute the Function App.
    /// </summary>
    public class EnvironmentVariablesModel : IEnvironmentVariablesModel
    {
        // Application Insights

        /// <inheritdoc/>
        public string Environment { get; private set; } = EnvironmentVariablesHelper.GetSettings("ai:environment");

        /// <inheritdoc/>
        public string AppInsightsConnectionString { get; private set; } = EnvironmentVariablesHelper.GetSettings("APPLICATIONINSIGHTS_CONNECTION_STRING");

        // Azure Search

        /// <inheritdoc/>
        public string AsName { get; private set; } = EnvironmentVariablesHelper.GetSettings("as:name");

        /// <inheritdoc/>
        public string AsAdminKey { get; private set; } = EnvironmentVariablesHelper.GetSettings("as:adminKey");

        // Cosmos DB

        /// <inheritdoc/>
        public string CosmosDbEndPoint { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:endpointUri");

        /// <inheritdoc/>
        public string CosmosDbKey { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:endpointKey");

        /// <inheritdoc/>
        public string CosmosDbName { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:dbName");

        /// <inheritdoc/>
        public string CosmosFundingGroupCollectionName { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:fundingGroupCollectionName");

        /// <inheritdoc/>
        public string CosmosProviderFundingCollectionName { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:providerFundingCollectionName");

        /// <inheritdoc/>
        public string CosmosAuditCollectionName { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:auditCollectionName");

        /// <inheritdoc/>
        public int CosmosThroughputSize { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("cdb:throughputSize", 400);

        /// <inheritdoc/>
        public int CosmosThroughputWaitTimeSeconds { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("cdb:throughputWaitTimeSeconds", 0);

        /// <inheritdoc/>
        public bool CosmosProgramaticallyChangeThroughput { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsBool("cdb:programaticallyChangeThroughput", false);

        /// <inheritdoc/>
        public int SimultanousCosmosReadWriteCount { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("cdb:simultanousCosmosReadWriteCount", 50);

        /// <inheritdoc/>
        public string CosmosConnectionMode { get; private set; } = EnvironmentVariablesHelper.GetSettings("cdb:ConnectionMode", "Direct");

        // Funding's API authentication

        /// <inheritdoc/>
        public bool AuthUseAuthentication { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsBool("auth:useAuthentication", false);

        /// <inheritdoc/>
        public string AuthAuthority { get; private set; } = EnvironmentVariablesHelper.GetSettings("auth:authority");

        /// <inheritdoc/>
        public string AuthTenantId { get; private set; } = EnvironmentVariablesHelper.GetSettings("auth:tenantId");

        /// <inheritdoc/>
        public string AuthClientId { get; private set; } = EnvironmentVariablesHelper.GetSettings("auth:clientId");

        /// <inheritdoc/>
        public string AuthClientSecret { get; private set; } = EnvironmentVariablesHelper.GetSettings("auth:clientSecret");

        /// <inheritdoc/>
        public string AuthAppIdUri { get; private set; } = EnvironmentVariablesHelper.GetSettings("auth:appIdUri");

        // VYF API

        /// <inheritdoc/>
        public string VyfBaseUri { get; private set; } = EnvironmentVariablesHelper.GetSettings("vyf:baseUri");

        /// <inheritdoc/>
        public string AutoPullEndpointUri { get; private set; } = EnvironmentVariablesHelper.GetSettings("vyf:autoPullEndpointUri");

        /// <inheritdoc/>
        public string VyfApiKey { get; private set; } = EnvironmentVariablesHelper.GetSettings("vyf:apiKey");

        // Others

        /// <inheritdoc/>
        public int TaskBatchSize { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("taskBatchSize", 3);

        /// <inheritdoc/>
        public string FundingsApiUri { get; private set; } = EnvironmentVariablesHelper.GetSettings("fundingsApi:baseUrl");

        /// <inheritdoc/>
        public int NumberOfFundingsToRetrieveFromApi { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("fundingsApi:pageSize", 500);

        /// <inheritdoc/>
        public string RunMode { get; private set; } = EnvironmentVariablesHelper.GetSettings("runMode")?.ToLower();

        /// <inheritdoc/>
        public int MaxConnectionsPerServer { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("maxConnectionsPerServer", 5);

        /// <inheritdoc/>
        public int HttpTimeoutInMinutes { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("httpTimeoutInMinutes", 60);

        /// <inheritdoc/>
        public int MaximumPagesToProcess { get; private set; } = EnvironmentVariablesHelper.GetSettingsAsInt("maximumPagesToProcess", int.MaxValue);
    }
}
