using Newtonsoft.Json;
using System;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Parent organisation details.
    /// </summary>
    public class ParentInformation
    {
        /// <summary>
        /// Gets or sets the organisation group type (UKPRN or LACode).
        /// </summary>
        [JsonProperty("group")]
        public OrganisationGroup Group { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the allocation can be published externally.
        /// </summary>
        [JsonProperty("externalPublicationDate")]
        public DateTimeOffset ExternalPublicationDate { get; set; }

        /// <summary>
        /// Gets or sets date and time when the allocation was published.
        /// </summary>
        [JsonProperty("statusChangedDate")]
        public DateTimeOffset StatusChangedDate { get; set; }

        /// <summary>
        /// Gets or sets the id for Parent Information.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the grouping reason.
        /// Does the grouping reflect how the money is paid ('Payment') or is it just useful to show it this way? ('Informational').
        /// </summary>
        [JsonProperty("groupingReason")]
        public string GroupingReason { get; set; }
    }
}