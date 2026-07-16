using Newtonsoft.Json;
using System;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// 2nd level (atomEntry) object in an atom feed.
    /// </summary>
    public class FeedResponseContentModel
    {
        /// <summary>
        /// Gets or sets the id of the feed atom entry.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the entry.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the author of the feed entry.
        /// </summary>
        [JsonProperty("author")]
        public FeedResponseAuthor Author { get; set; }

        /// <summary>
        /// Gets or sets when the feed entry was updated / created.
        /// </summary>
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Gets or sets atom feed link.
        /// </summary>
        [JsonProperty("link")]
        public FeedLink Link { get; set; }

        /// <summary>
        /// Gets or sets content of the feed entry (the funding).
        /// </summary>
        [JsonProperty("content")]
        public FeedBaseModel Content { get; set; }
    }
}