using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Top level object for an atom feed.
    /// </summary>
    public class FeedResponseModel
    {
        /// <summary>
        /// Gets or sets id of the feed (not used in our case).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets feed title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets author of the feed.
        /// </summary>
        [JsonProperty("author")]
        public FeedResponseAuthor Author { get; set; }

        /// <summary>
        /// Gets or sets when the feed was updated (newest updated date of any funding).
        /// </summary>
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Gets or sets copyright information.
        /// </summary>
        [JsonProperty("rights")]
        public string Rights { get; set; }

        /// <summary>
        /// Gets or sets array of relational links.
        /// </summary>
        [JsonProperty("link")]
        public List<FeedLink> Link { get; set; }

        /// <summary>
        /// Gets or sets array of entries that ultimately contain fundings.
        /// </summary>
        [JsonProperty("atomEntry")]
        public FeedResponseContentModel[] AtomEntry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether feed was cancelled.
        /// </summary>
        [JsonProperty("cancelled")]
        public bool Cancelled { get; set; }
    }
}