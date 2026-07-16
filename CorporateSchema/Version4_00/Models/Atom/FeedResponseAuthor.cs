using Newtonsoft.Json;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// Information about the author of the feed.
    /// </summary>
    public class FeedResponseAuthor
    {
        /// <summary>
        /// Gets or sets the email address of the author.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}