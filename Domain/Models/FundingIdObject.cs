using Newtonsoft.Json;

namespace Domain.Models
{
    /// <summary>
    /// An object simply containing a funding id property.
    /// </summary>
    public class FundingIdObject
    {
        /// <summary>
        /// Gets or sets the funding ID.
        /// </summary>
        [JsonProperty("fundingId")]
        public string FundingId { get; set; }
    }
}