using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
    /// </summary>
    public class FundingProvider : Funding
    {
        /// <summary>
        /// Gets or sets the fundings (child organisation level lines, e.g. providers under an LA) that are grouped into this funding group.
        /// </summary>
        [JsonProperty("providerFundings", Order = 8)]
        public IEnumerable<ProviderFunding> ProviderFundings { get; set; }
    }
}