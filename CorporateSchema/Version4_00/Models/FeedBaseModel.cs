using Newtonsoft.Json;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Base model for the feed method.
    /// </summary>
    public class FeedBaseModel
    {
        /// <summary>
        /// Gets or sets the schema URI to validate against.
        /// </summary>
        [JsonProperty("$schema")]
        public string SchemaUri { get; set; }

        /// <summary>
        /// Gets or sets the schema version. Schema here refers to the Classes etc.., not the specific model being used.
        /// </summary>
        [JsonProperty("schemaVersion")]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
        /// </summary>
        [JsonProperty("funding")]
        public FundingFeed Funding { get; set; }
    }
}