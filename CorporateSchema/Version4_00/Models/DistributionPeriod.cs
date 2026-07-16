using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Funding values grouped by the distribution period (envelope) they are paid in.
    /// </summary>
    public class DistributionPeriod
    {
        /// <summary>
        /// Gets or sets the overall value for the distribution period in pence. Rolled up from all child Funding Lines where Type = Payment.
        /// </summary>
        [JsonProperty("value")]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the funding period the funding relates to.
        /// </summary>
        [JsonProperty("distributionPeriodId")]
        public string DistributionPeriodId { get; set; }

        /// <summary>
        /// Gets or sets the periods that this funding line were paid in / are due to be paid in.
        /// </summary>
        [JsonProperty("profilePeriods", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ProfilePeriod> ProfilePeriods { get; set; }
    }
}