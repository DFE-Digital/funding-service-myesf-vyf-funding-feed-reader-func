using Newtonsoft.Json;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A key/value pairing representing a organisation identifier.
    /// </summary>
    public class OrganisationIdentifier
    {
        /// <summary>
        /// Gets or sets the type of organisation identifier (e.g. UKPRN).
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value of this identifier type (e.g. if the type is UKPRN, then the value may be 12345678.
        /// If the type is LECode, the value may be 'LA 203').
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}