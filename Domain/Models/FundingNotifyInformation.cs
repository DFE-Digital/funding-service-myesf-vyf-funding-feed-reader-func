using Microsoft.WindowsAzure.Storage.Table;

namespace Domain.Models
{
    /// <summary>
    /// Table entity to store funding notify information.
    /// </summary>
    public class FundingNotifyInformation : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingNotifyInformation"/> class.
        /// This table entity uses funding id as the row key.
        /// Funding type and funding id uniquely identify rows of this table entity.
        /// </summary>
        /// <param name="fundingId">The funding id.</param>
        /// <param name="type">Whether funding or provider funding type.</param>
        public FundingNotifyInformation(string fundingId, string type)
            : base(type, fundingId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingNotifyInformation"/> class.
        /// The default constructor for deserialization.
        /// </summary>
        public FundingNotifyInformation()
        {
        }

        /// <summary>
        /// Gets or sets the datetime when the email was sent for new funding created.
        /// </summary>
        public string EmailSentAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the status of email for the funding.
        /// </summary>
        public string EmailStatus { get; set; }
    }
}
