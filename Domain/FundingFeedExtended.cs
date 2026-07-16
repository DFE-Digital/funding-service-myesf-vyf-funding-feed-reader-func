using CorporateSchema.Version4_00;

namespace Domain
{
    /// <summary>
    /// FundingFeed data, with the addition of whether it exists in our Cosmos or not.
    /// </summary>
    public class FundingFeedExtended
    {
        /// <summary>
        /// Gets or sets the funding feed object.
        /// </summary>
        public FundingFeed FundingFeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it exists in the Cosmos collection or not.
        /// </summary>
        public bool ExistsInCosmosDb { get; set; }
    }
}