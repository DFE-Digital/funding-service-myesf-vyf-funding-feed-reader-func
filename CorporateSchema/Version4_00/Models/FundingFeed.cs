using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
    /// </summary>
    public class FundingFeed : Funding
    {
        /// <summary>
        /// Gets or sets a list of providerFundings.
        /// The fundings (child organisation level lines, e.g. providers under an LA) that are grouped into this funding group.
        /// </summary>
        [JsonProperty("providerFundings", Order = 8)]
        public IEnumerable<string> ProviderFundings { get; set; }

        /// <summary>
        /// Gets or sets partition key.
        /// </summary>
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the schema version number.
        /// </summary>
        [JsonProperty("schemaVersion")]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the scenario id used to track through the source of the data.
        /// </summary>
        [JsonProperty("scenarioId")]
        public string ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the scenario id used to track through the source of the data.
        /// </summary>
        [JsonProperty("variationReasons")]
        public string[] VariationReasons { get; set; }

        /// <summary>
        /// Gets or sets the date time the funding was created.
        /// </summary>
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}