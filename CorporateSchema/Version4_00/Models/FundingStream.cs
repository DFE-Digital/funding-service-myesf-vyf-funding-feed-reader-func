using Newtonsoft.Json;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Details around a funding stream.
    /// </summary>
    public class FundingStream
    {
        /// <summary>
        /// Gets or sets the code for the funding stream (e.g. PESport).
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the name of the funding stream (e.g. PE Sport &amp; Premium).
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}