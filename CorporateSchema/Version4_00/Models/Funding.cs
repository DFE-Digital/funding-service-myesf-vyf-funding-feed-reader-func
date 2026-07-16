using CorporateSchema.Version4_00.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
    /// </summary>
    public abstract class Funding
    {
        /// <summary>
        /// Gets or sets the Id.
        /// Unique identifier of this funding group / business event (in format 'FundingStreamCode-FundingPeriodId-OrganisationGroupGroupTypeCode-OrganisationGroupIdentifierValue-FundingVersion').
        /// </summary>
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the version of the template (e.g. this is Version 2 of PE and sport template).
        /// </summary>
        [JsonProperty("templateVersion")]
        public string TemplateVersion { get; set; }

        /// <summary>
        /// Gets or sets funding version.
        /// Version number of the published data. If there are changes to the funding for this organisation in this period, this number would increase.
        /// Major and minor are separated by an underscore e.g. 1_0.
        /// </summary>
        [JsonProperty("fundingVersion", Order = 2)]
        public string FundingVersion { get; set; }

        /// <summary>
        /// Gets or sets version number of the published data. If there are changes to the funding for this organisation in this period, this number would increase.
        /// </summary>
        [JsonProperty("channelVersion")]
        public IEnumerable<ChannelVersion> ChannelVersion { get; set; }

        /// <summary>
        /// Gets or sets the funding status (i.e. published).
        /// </summary>
        [JsonProperty("status", Order = 3)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the funding stream the funding relates to.
        /// </summary>
        [JsonProperty("fundingStream", Order = 4)]
        public FundingStream FundingStream { get; set; }

        /// <summary>
        /// Gets or sets the funding period the funding relates to.
        /// </summary>
        [JsonProperty("fundingPeriod", Order = 5)]
        public FundingPeriod FundingPeriod { get; set; }

        /// <summary>
        /// Gets or sets the grouped organisation or region (e.g. if we are grouping by LA, the organisation may be Camden).
        /// </summary>
        [JsonProperty("organisationGroup", Order = 6)]
        public OrganisationGroup OrganisationGroup { get; set; }

        /// <summary>
        /// Gets or sets funding value breakdown.
        /// </summary>
        [JsonProperty("fundingValue", Order = 7)]
        public FundingValue FundingValue { get; set; }

        /// <summary>
        /// Gets or sets the grouping reason.
        /// Does the grouping reflect how the money is paid ('Payment') or is it just useful to show it this way? ('Informational').
        /// </summary>
        [JsonProperty("groupingReason", Order = 9)]
        public string GroupingReason { get; set; }

        /// <summary>
        /// Gets or sets the date the funding was published by a business user.
        /// </summary>
        [JsonProperty("statusChangedDate", Order = 10)]
        public DateTimeOffset StatusChangedDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the allocation can be published externally.
        /// </summary>
        [JsonProperty("externalPublicationDate", Order = 11)]
        public DateTimeOffset ExternalPublicationDate { get; set; }

        /// <summary>
        /// Gets or sets the earliest date the payment will be made available to pay to the provider.
        /// </summary>
        [JsonProperty("earliestPaymentAvailableDate", Order = 12)]
        public DateTimeOffset? EarliestPaymentAvailableDate { get; set; }
    }
}