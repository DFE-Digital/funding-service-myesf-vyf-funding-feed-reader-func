using CorporateSchema.Version4_00;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A funding line.
    /// </summary>
    public class FundingLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingLine"/> class.
        /// </summary>
        public FundingLine()
        {
        }

        /// <summary>
        /// Gets or sets the name of a funding line (e.g. "Total funding line").
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the funding line code.
        /// Unique code within the template to lookup this specific funding line.
        /// Used to map this funding line in consuming systems (e.g. NAV for payment).
        /// </summary>
        [JsonProperty("fundingLineCode", NullValueHandling = NullValueHandling.Ignore)]
        public string FundingLineCode { get; set; }

        /// <summary>
        /// Gets or sets the funding value in pence.
        /// </summary>
        [JsonProperty("value")]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the template line ID.
        /// A unique ID (in terms of template, not data) for this funding line (e.g. 345).
        /// </summary>
        [JsonProperty("templateLineId")]
        public uint TemplateLineId { get; set; }

        /// <summary>
        /// Gets or sets the type of funding line (e.g. paid on this basis, or informational only).
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the periods that this funding line where paid in / are due to be paid in.
        /// </summary>
        [JsonProperty("distributionPeriods", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DistributionPeriod> DistributionPeriods { get; set; }

        /// <summary>
        /// Gets or sets calculations that make up this funding line.
        /// </summary>
        [JsonProperty("calculations", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Calculation> Calculations { get; set; }

        /// <summary>
        /// Gets or sets sub funding lines that make up this funding line.
        /// </summary>
        [JsonProperty("fundingLines", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FundingLine> FundingLines { get; set; }
    }
}