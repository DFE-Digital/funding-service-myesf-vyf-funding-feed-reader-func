using CorporateSchema.Version4_00.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A funding item.
    /// </summary>
    public class ProviderFunding
    {
        /// <summary>
        /// Gets or sets a unique id for this funding.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets version number of the published data. If there are changes to the funding for this organisation in this period, this number would increase.
        /// </summary>
        [JsonProperty("fundingVersion")]
        public string FundingVersion { get; set; }

        /// <summary>
        /// Gets or sets version number of the published data. If there are changes to the funding for this organisation in this period, this number would increase.
        /// </summary>
        [JsonProperty("channelVersion")]
        public IEnumerable<ChannelVersion> ChannelVersion { get; set; }

        /// <summary>
        /// Gets or sets the organisation for which the funding is for.
        /// </summary>
        [JsonProperty("provider")]
        public Provider Provider { get; set; }

        /// <summary>
        /// Gets or sets the funding stream the funding relates to.
        /// </summary>
        [JsonProperty("fundingStreamCode")]
        public string FundingStreamCode { get; set; }

        /// <summary>
        /// Gets or sets the funding period the funding relates to. e.g. AY-1819.
        /// </summary>
        [JsonProperty("fundingPeriodId")]
        public string FundingPeriodId { get; set; }

        /// <summary>
        /// Gets or sets funding value.
        /// </summary>
        [JsonProperty("fundingValue")]
        public FundingValue FundingValue { get; set; }

        /// <summary>
        /// Gets or sets optional reasons for the provider variation. These reasons are in addition to open and close reason of the organisation.
        /// This field can contain zero or more items.
        /// </summary>
        [JsonProperty("variationReasons")]
        public IEnumerable<string> VariationReasons { get; set; }

        /// <summary>
        /// Gets or sets collection of successor providers.
        /// </summary>
        [JsonProperty("successors")]
        public IEnumerable<JToken> Successors { get; set; }

        /// <summary>
        /// Gets or sets collection of predecessor providers.
        /// </summary>
        [JsonProperty("predecessors")]
        public IEnumerable<JToken> Predecessors { get; set; }

        private string ConvertVersionForId(string version)
        {
            return version.Replace(".", "_");
        }
    }
}