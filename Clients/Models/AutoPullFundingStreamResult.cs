using Newtonsoft.Json;

namespace Clients.Models
{
    /// <summary>
    /// The AutoPullFundingStreamResult receievd from external api.
    /// </summary>
    public class AutoPullFundingStreamResult
    {
        /// <summary>
        /// Gets or sets the funding stream code found.
        /// </summary>
        [JsonProperty("fundingStreamCode")]
        public string FundingStreamCode { get; set; }

        /// <summary>
        /// Gets or sets the funding stream name found.
        /// </summary>
        [JsonProperty("fundingStreamName")]
        public string FundingStreamName { get; set; }
    }
}
