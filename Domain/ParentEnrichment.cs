using Newtonsoft.Json;

namespace Domain
{
    /// <summary>
    /// Parent info that comes out of a Cosmos DB document - along with the info about its parent.
    /// </summary>
    public class ParentEnrichment : ParentEnrichmentWithoutProviderFundingId
    {
        /// <summary>
        /// Gets or sets the ID of the provider funding.
        /// </summary>
        [JsonProperty("pfid")]
        public string ProviderFundingId { get; set; }
    }
}