using Newtonsoft.Json;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Atom feeds link to previous, next, first etc...
    /// </summary>
    public class FeedLink
    {
        /// <summary>
        /// Gets or sets the URI for the relational page.
        /// </summary>
        [JsonProperty("href")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the type of page (first, last, self, prev-archive, last).
        /// </summary>
        [JsonProperty("rel")]
        public string Rel { get; set; }
    }
}