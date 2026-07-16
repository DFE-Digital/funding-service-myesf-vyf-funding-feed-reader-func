using CorporateSchema.Version4_00;
using Newtonsoft.Json;
using System;

namespace Domain
{
    /// <summary>
    /// Parent info that comes out of a Cosmos DB document.
    /// </summary>
    public class ParentEnrichmentWithoutProviderFundingId
    {
        /// <summary>
        /// Gets or sets the ID of the parent funding.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets date time that the funding can be display to the public (according to the model).
        /// </summary>
        [JsonProperty("externalPublicationDate")]
        public DateTimeOffset ExternalPublicationDate { get; set; }

        /// <summary>
        /// Gets or sets date time that the funding was published.
        /// </summary>
        [JsonProperty("statusChangedDate")]
        public DateTimeOffset StatusChangedDate { get; set; }

        /// <summary>
        /// Gets or sets the organisation group (e.g. the parent LA etc...).
        /// </summary>
        [JsonProperty("group")]
        public OrganisationGroup Group { get; set; }

        /// <summary>
        /// Gets or sets the grouping reason (usually Payment or Information).
        /// </summary>
        [JsonProperty("groupingReason")]
        public string GroupingReason { get; set; }
    }
}