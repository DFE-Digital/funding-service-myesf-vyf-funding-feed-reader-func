using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// The total amount paid, and the periods/envelopes that it was composed of.
    /// </summary>
    public class FundingValue
    {
        /// <summary>
        /// Gets or sets the funding value amount in pence. Rolled up from all child Funding Lines where Type = Payment.
        /// </summary>
        [JsonProperty("totalValue")]
        public double TotalValue { get; set; }

        /// <summary>
        /// Gets or sets the lines that make up this funding.
        /// </summary>
        [JsonProperty("fundingLines")]
        public JToken FundingLines { get; set; }

        /// <summary>
        /// Gets or sets the calculations that make up this funding.
        /// </summary>
        [JsonProperty("calculations")]
        public JToken Calculations { get; set; }
    }
}