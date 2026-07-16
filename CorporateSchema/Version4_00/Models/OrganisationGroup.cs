using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A grouping organisation (e.g. 'Camden', an LA) or (specific provider, 100023) or (country England).
    /// </summary>
    public class OrganisationGroup
    {
        /// <summary>
        /// Gets or sets the organisation group type. e.g. UKPRN or LACode.
        /// </summary>
        [JsonProperty("groupTypeCode")]
        public string GroupTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the organisation group type. e.g. UKPRN or LACode.
        /// </summary>
        [JsonProperty("groupTypeIdentifier")]
        public JToken GroupTypeIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the organisation group type. e.g. UKPRN or LACode.
        /// </summary>
        [JsonProperty("groupTypeCategory")]
        public string GroupTypeClassification { get; set; }

        /// <summary>
        /// Gets or sets the name of the grouping organisation (e.g. in the case of the type being LA, this could be 'Camden', "Bermondsey and Old Southwark").
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
        [JsonProperty("identifiers")]
        public IEnumerable<OrganisationIdentifier> Identifiers { get; set; }
    }
}