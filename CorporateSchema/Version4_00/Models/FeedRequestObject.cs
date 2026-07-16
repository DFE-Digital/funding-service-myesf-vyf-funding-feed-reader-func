using Newtonsoft.Json;
using System;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Optional feed request parameters.
    /// </summary>
    public class FeedRequestObject
    {
        /// <summary>
        /// Gets or sets the page size to fetch (optional).
        /// </summary>
        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the period start year (optional).
        /// </summary>
        [JsonProperty("fundingPeriodStartYear")]
        public int? FundingPeriodStartYear { get; set; }

        /// <summary>
        /// Gets or sets the period end year (optional).
        /// </summary>
        [JsonProperty("fundingPeriodEndYear")]
        public int? FundingPeriodEndYear { get; set; }

        /// <summary>
        /// Gets or sets the funding period codes to restrict to (optional).
        /// </summary>
        [JsonProperty("fundingPeriodCodes")]
        public string[] FundingPeriodCodes { get; set; }

        /// <summary>
        /// Gets or sets the group identifiers to filter by.
        /// </summary>
        [JsonProperty("organisationGroupIdentifiers")]
        public ProviderIdentifier[] OrganisationGroupIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets group types to limit to.
        /// </summary>
        [JsonProperty("organisationGroupTypes")]
        public string[] OrganisationGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets provider identifiers.
        /// Restrict returned identifiers by id (optional).
        /// </summary>
        [JsonProperty("organisationIdentifiers")]
        public ProviderIdentifier[] OrganisationIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets organisation types to limit to (optional).
        /// </summary>
        [JsonProperty("organisationTypes")]
        public string[] OrganisationTypes { get; set; }

        /// <summary>
        /// Gets or sets variation reasons to limit to (optional).
        /// </summary>
        [JsonProperty("variationReasons")]
        public string[] VariationReasons { get; set; }

        /// <summary>
        /// Gets or sets UKPRN's to limit to.
        /// </summary>
        [JsonProperty("ukPrns")]
        public string[] Ukprns { get; set; }

        /// <summary>
        /// Gets or sets grouping reasons to limit to e.g. Information or Payment (optional).
        /// </summary>
        [JsonProperty("groupingReasons")]
        public string[] GroupingReasons { get; set; }

        /// <summary>
        /// Gets or sets statuses to limit to e.g. Released (optional).
        /// </summary>
        [JsonProperty("statuses")]
        public string[] Statuses { get; set; }

        /// <summary>
        /// Gets or sets minimum status change date.
        /// Only get data that was changed after this date (optional).
        /// </summary>
        [JsonProperty("minStatusChangedDate")]
        public DateTime? MinStatusChangeDate { get; set; }

        /// <summary>
        /// Gets or sets funding stream codes to limit to (optional).
        /// </summary>
        [JsonProperty("fundingStreamCodes")]
        public string[] FundingStreamCodes { get; set; }

        /// <summary>
        /// Gets or sets funding line types.
        /// Only get back funding lines with these types (optional).
        /// </summary>
        [JsonProperty("fundingLineTypes")]
        public string[] FundingLineTypes { get; set; }

        /// <summary>
        /// Gets or sets the template line Id's.
        /// Only get funding lines with these ids (optional).
        /// </summary>
        [JsonProperty("templateLineIds")]
        public string[] TemplateLineIds { get; set; }
    }
}