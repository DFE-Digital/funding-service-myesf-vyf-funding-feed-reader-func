using Newtonsoft.Json;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A funding line profile period (e.g. the 1st March payment in 2019), with relevant value data.
    /// The composite key for the entity is Type, TypeValue, Year and Occurrence.
    /// </summary>
    public class ProfilePeriod
    {
        /// <summary>
        /// Gets or sets the type of the period (e.g. CalendarMonth).
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value identifier for this period (e.g. if type is 'Calendar Month', this could be 'April').
        /// </summary>
        [JsonProperty("typeValue")]
        public string TypeValue { get; set; }

        /// <summary>
        /// Gets or sets which year is the period in.
        /// </summary>
        [JsonProperty("year")]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets which occurrence this month (note that this is 1 indexed).
        /// Use this to support multiple Funding Line Periods/Profiles in a single Type/TypeValue period
        /// e.g. April 2020 when three payments are made in this month, the ProfilePeriods array will have three FundingLinePeriods returned in the array with Occurrence set to 1, 2 and 3.
        /// </summary>
        [JsonProperty("occurrence")]
        public int Occurrence { get; set; }

        /// <summary>
        /// Gets or sets the amount of the profiled value, in pence.
        /// </summary>
        [JsonProperty("profiledValue")]
        public long ProfiledValue { get; set; }

        /// <summary>
        /// Gets or sets the funding period code for the funding. e.g. FY-2020. This will match the distribution period this profile is paid in.
        /// </summary>
        [JsonProperty("distributionPeriodId")]
        public string DistributionPeriodId { get; set; }
    }
}