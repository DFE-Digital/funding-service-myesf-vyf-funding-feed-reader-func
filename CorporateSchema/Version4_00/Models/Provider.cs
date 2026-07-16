using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// An organisation.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// Gets or sets the ID of the organisation. This is the UKRPN of the provider.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets text for Azure search to make this entity searchable. This is the name, but with punctuation etc removed to make it suitable for searching.
        /// </summary>
        [JsonProperty("searchableName")]
        public string SearchableName { get; set; }

        /// <summary>
        /// Gets or sets identifier numbers for this organisation.
        /// </summary>
        [JsonProperty("otherIdentifiers")]
        public IEnumerable<ProviderIdentifier> OtherIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets iD on the provider lookup API.
        /// </summary>
        [JsonProperty("providerVersionId")]
        public string ProviderVersionId { get; set; }

        /// <summary>
        /// Gets or sets provider type (e.g. School, Academy, Special School) - not enumerated as this isn't controlled by CFS, but passed through from the Provider info (GIAS).
        /// </summary>
        [JsonProperty("providerType")]
        public string ProviderType { get; set; }

        /// <summary>
        /// Gets or sets provider sub type (TODO - list enumerations).
        /// </summary>
        [JsonProperty("providerSubType")]
        public string ProviderSubType { get; set; }

        /// <summary>
        /// Gets or sets provider Details. This property is optional.
        /// </summary>
        [JsonProperty("providerDetails")]
        public ProviderDetails ProviderDetails { get; set; }
    }
}