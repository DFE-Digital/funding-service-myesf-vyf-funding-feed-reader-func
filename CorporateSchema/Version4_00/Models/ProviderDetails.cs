using Newtonsoft.Json;
using System;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// (Optional) details about a provider. Passed through from the provider API.
    /// </summary>
    public class ProviderDetails
    {
        /// <summary>
        /// Gets or sets date Opened.
        /// </summary>
        [JsonProperty("dateOpened")]
        public DateTimeOffset? DateOpened { get; set; }

        /// <summary>
        /// Gets or sets date Closed.
        /// </summary>
        [JsonProperty("dateClosed")]
        public DateTimeOffset? DateClosed { get; set; }

        /// <summary>
        /// Gets or sets status of the organisation.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets phase of Education.
        /// </summary>
        [JsonProperty("phaseOfEducation")]
        public string PhaseOfEducation { get; set; }

        /// <summary>
        /// Gets or sets local Authority Name.
        /// </summary>
        [JsonProperty("localAuthorityName")]
        public string LocalAuthorityName { get; set; }

        /// <summary>
        /// Gets or sets optional open reason from the list of GIAS Open Reasons.
        /// </summary>
        [JsonProperty("openReason")]
        public string OpenReason { get; set; }

        /// <summary>
        /// Gets or sets optional close reason from list of GIAS Close Reasons.
        /// </summary>
        [JsonProperty("closeReason")]
        public string CloseReason { get; set; }

        /// <summary>
        /// Gets or sets trust Status.
        /// </summary>
        [JsonProperty("trustStatus")]
        public string TrustStatus { get; set; }

        /// <summary>
        /// Gets or sets trust Name.
        /// </summary>
        [JsonProperty("trustName")]
        public string TrustName { get; set; }

        /// <summary>
        /// Gets or sets town.
        /// </summary>
        [JsonProperty("town")]
        public string Town { get; set; }

        /// <summary>
        /// Gets or sets postcode.
        /// </summary>
        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets companies House Number.
        /// </summary>
        [JsonProperty("companiesHouseNumber")]
        public string CompaniesHouseNumber { get; set; }

        /// <summary>
        /// Gets or sets group ID.
        /// </summary>
        [JsonProperty("groupIdNumber")]
        public string GroupIDNumber { get; set; }

        /// <summary>
        /// Gets or sets rSC Region Name.
        /// </summary>
        [JsonProperty("rscRegionName")]
        public string RSCRegionName { get; set; }

        /// <summary>
        /// Gets or sets rSC Region Code.
        /// </summary>
        [JsonProperty("rscRegionCode")]
        public string RSCRegionCode { get; set; }

        /// <summary>
        /// Gets or sets government Office Region Name.
        /// </summary>
        [JsonProperty("governmentOfficeRegionName")]
        public string GovernmentOfficeRegionName { get; set; }

        /// <summary>
        /// Gets or sets government Office Region Code.
        /// </summary>
        [JsonProperty("governmentOfficeRegionCode")]
        public string GovernmentOfficeRegionCode { get; set; }

        /// <summary>
        /// Gets or sets district Name.
        /// </summary>
        [JsonProperty("districtName")]
        public string DistrictName { get; set; }

        /// <summary>
        /// Gets or sets district Code.
        /// </summary>
        [JsonProperty("districtCode")]
        public string DistrictCode { get; set; }

        /// <summary>
        /// Gets or sets ward Name.
        /// </summary>
        [JsonProperty("wardName")]
        public string WardName { get; set; }

        /// <summary>
        /// Gets or sets ward Code.
        /// </summary>
        [JsonProperty("wardCode")]
        public string WardCode { get; set; }

        /// <summary>
        /// Gets or sets census Ward Name.
        /// </summary>
        [JsonProperty("censusWardName")]
        public string CensusWardName { get; set; }

        /// <summary>
        /// Gets or sets census Ward Code.
        /// </summary>
        [JsonProperty("censusWardCode")]
        public string CensusWardCode { get; set; }

        /// <summary>
        /// Gets or sets middle Super Output Area Name.
        /// </summary>
        [JsonProperty("middleSuperOutputAreaName")]
        public string MiddleSuperOutputAreaName { get; set; }

        /// <summary>
        /// Gets or sets middle Super Output Area Code.
        /// </summary>
        [JsonProperty("middleSuperOutputAreaCode")]
        public string MiddleSuperOutputAreaCode { get; set; }

        /// <summary>
        /// Gets or sets lower Super Output Area Name.
        /// </summary>
        [JsonProperty("lowerSuperOutputAreaName")]
        public string LowerSuperOutputAreaName { get; set; }

        /// <summary>
        /// Gets or sets lower Super Output Area Code.
        /// </summary>
        [JsonProperty("lowerSuperOutputAreaCode")]
        public string LowerSuperOutputAreaCode { get; set; }

        /// <summary>
        /// Gets or sets parliamentary Constituency Name.
        /// </summary>
        [JsonProperty("parliamentaryConstituencyName")]
        public string ParliamentaryConstituencyName { get; set; }

        /// <summary>
        /// Gets or sets parliamentary Constituency Code.
        /// </summary>
        [JsonProperty("parliamentaryConstituencyCode")]
        public string ParliamentaryConstituencyCode { get; set; }

        /// <summary>
        /// Gets or sets country Code.
        /// </summary>
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets country Name.
        /// </summary>
        [JsonProperty("countryName")]
        public string CountryName { get; set; }
    }
}