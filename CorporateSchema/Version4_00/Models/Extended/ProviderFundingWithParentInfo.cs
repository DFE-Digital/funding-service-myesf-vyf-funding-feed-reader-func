using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Provider funding extended to have list of Parent organisation details.
    /// </summary>
    public class ProviderFundingWithParentInfo : ProviderFunding
    {
        /// <summary>
        /// Gets or sets the list of parent organisation details.
        /// </summary>
        [JsonProperty("parentGroups")]
        public List<ParentInformation> ParentInformation { get; set; }
    }
}