using CorporateSchema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Class to capture results of feed reader run.
    /// </summary>
    public class FeedReaderResultReport
    {
        /// <summary>
        /// Gets or sets the ID of the audit history.
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the 'action' (Import, Clear etc...).
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets feed reader start date-time.
        /// </summary>
        [JsonProperty("startDateTime")]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets feed reader completion date-time.
        /// </summary>
        [JsonProperty("endDateTime")]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets feed reader last updated date-time.
        /// </summary>
        [JsonProperty("lastUpdatedDateTime")]
        public DateTime LastUpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets status of feed reader run.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a funding stream list, comma seperated (optional).
        /// </summary>
        [JsonProperty("fundingStreams")]
        public string FundingStreams { get; set; }

        /// <summary>
        /// Gets or sets a funding period code list, comma seperated (optional).
        /// </summary>
        [JsonProperty("fundingPeriod")]
        public string FundingPeriods { get; set; }

        /// <summary>
        /// Gets or sets a comma seperated list of scenario ids to use (optional).
        /// </summary>
        [JsonProperty("scenarioId")]
        public string ScenarioIds { get; set; }

        /// <summary>
        /// Gets or sets additional information related to feed reader run.
        /// </summary>
        [JsonProperty("additionalInformation")]
        public string AdditionalInformation { get; set; }

        /// <summary>
        /// Gets or sets cFS Fundings API Uri.
        /// </summary>
        [JsonProperty("fundingUri")]
        public string FundingUri { get; set; }

        /// <summary>
        /// Gets or sets cFS Provider fundings API Uri.
        /// </summary>
        [JsonProperty("providerFundingUri")]
        public string ProviderFundingUri { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current setting for programatically change throughput.
        /// If true change CosmosDB throughput size setting and revert back.
        /// when processing finishes.
        /// </summary>
        [JsonProperty("programaticallyChangeThroughput")]
        public bool ProgramaticallyChangeThroughput { get; set; }

        /// <summary>
        /// Gets or sets fundings in CosmosDB before feed reader ran.
        /// </summary>
        [JsonProperty("fundingsCosmosDbCountBeforeRun")]
        public long FundingsDbCountBefore { get; set; }

        /// <summary>
        ///  Gets or sets fundings found in provider fundings API.
        /// </summary>
        [JsonProperty("newFundingsCountInApi")]
        public int FundingsCountInApi { get; set; }

        /// <summary>
        /// Gets or sets fundings in CosmosDB after feed reader ran.
        /// </summary>
        [JsonProperty("fundingsCosmosDbCountAfterRun")]
        public long FundingsDbCountAfter { get; set; }

        /// <summary>
        /// Gets or sets provider fundings in CosmosDB before feed reader ran.
        /// </summary>
        [JsonProperty("providerFundingsCosmosDbCountBeforeRun")]
        public long ProviderFundingsDbCountBefore { get; set; }

        /// <summary>
        /// Gets or sets number of new provider fundings in fundings.
        /// </summary>
        [JsonProperty("newProviderFundingsInApiCount")]
        public int NewProviderFundingsInApiCount { get; set; }

        /// <summary>
        /// Gets or sets number of provider fundings in the feed, include any duplicates (i.e. provider funding showing under N different fundings).
        /// </summary>
        [JsonProperty("providerFundingsInApiCountIncludingDuplicates")]
        public int ProviderFundingsInApiCountIncludingDuplicates { get; set; }

        /// <summary>
        /// Gets or sets number of provider fundings in the feed, excluding duplicates (i.e. provider funding showing under N different fundings).
        /// </summary>
        [JsonProperty("providerFundingsInApiCountDistinct")]
        public int ProviderFundingsInApiCountDistinct { get; set; }

        /// <summary>
        /// Gets or sets provider fundings in CosmosDB after feed reader run.
        /// </summary>
        [JsonProperty("providerFundingsCosmosDbCountAfterRun")]
        public long ProviderFundingsDbCountAfter { get; set; }

        /// <summary>
        /// Gets number of provider fundings that were not found in API.
        /// </summary>
        [JsonProperty("providerFundingsErrorCount")]
        public int ProviderFundingsErrorCount => FundingErrors.Count;

        /// <summary>
        /// Gets or sets partition key.
        /// </summary>
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets list of all provider fundings that were not found.
        /// </summary>
        [JsonProperty("fundingErrorsList")]
        public List<ProviderFundingError> FundingErrors { get; set; } = new List<ProviderFundingError>();

        /// <summary>
        /// Gets or sets the number of saves to the db.
        /// </summary>
        [JsonProperty("saveCount")]
        public int SaveCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the last processed page number.
        /// </summary>
        /// <value>
        /// The last processed page number.
        /// </value>
        [JsonProperty("lastProcessedPageNumber")]
        public int LastProcessedPageNumber { get; set; }

        /// <summary>
        /// Gets or sets the trigger method of the feed reader.
        /// </summary>
        [JsonProperty("trigger")]
        public string Trigger { get; set; }
    }
}
