using Newtonsoft.Json;
using System;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Details about the period.
    /// </summary>
    public class FundingPeriod
    {
        /// <summary>
        /// Gets or sets funding Period Id e.g. AY-2021.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the code for the period (e.g. 1920 or 2021).
        /// </summary>
        [JsonProperty("period")]
        public string Period { get; set; }

        /// <summary>
        /// Gets or sets the name of the period (e.g. Academic Year 2019-20).
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of period (academic or financial year).
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the start date for the period.
        /// </summary>
        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the period.
        /// </summary>
        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }
    }
}