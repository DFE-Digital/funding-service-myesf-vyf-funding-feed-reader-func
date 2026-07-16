using Newtonsoft.Json;

namespace CorporateSchema
{
    /// <summary>
    /// Report missing provider fundings.
    /// </summary>
    public class ProviderFundingError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFundingError" /> class.
        /// </summary>
        public ProviderFundingError()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFundingError" /> class.
        /// </summary>
        /// <param name="fundingId">Funding Id provider belongs to.</param>
        /// <param name="providerFundingId">Missing provider funding.</param>
        /// <param name="fundingUrl">Fundings Api that was called.</param>
        public ProviderFundingError(string fundingId, string providerFundingId, string fundingUrl)
        {
            FundingId = fundingId;
            ProviderFundingId = providerFundingId;
            FundingUri = fundingUrl;
        }

        /// <summary>
        /// Gets funding Id that provider funding belongs to.
        /// </summary>
        [JsonProperty("fundingId")]
        public string FundingId { get; private set; }

        /// <summary>
        /// Gets the Uri that funding was in.
        /// </summary>
        [JsonProperty("fundingUri")]
        public string FundingUri { get; private set; }

        /// <summary>
        /// Gets provider funding id that is missing.
        /// </summary>
        [JsonProperty("providerFundingId")]
        public string ProviderFundingId { get; private set; }
    }
}
