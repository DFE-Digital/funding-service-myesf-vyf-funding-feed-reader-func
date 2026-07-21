using Domain.Interfaces;
using Pds_azurefunction_fundingfeedreader.Helpers;

namespace Pds_azurefunction_fundingfeedreader.Models
{
    /// <summary>
    /// A Class to represent Feed Reader Input Uri.
    /// </summary>
    public class FeedReaderInputUriModel : IFeedReaderInputUriModel
    {
        private readonly string fundingsApiUri;
        private readonly int numberOfFundingsToRetrieveFromApi;
        private readonly string fundingStream;
        private readonly string apiVersionText = "api/v4";
        private readonly string channel = "statements";

        /// <inheritdoc/>
        public bool IsCFSUri => fundingsApiUri.IsCFSUri();

        /// <inheritdoc/>
        public bool IsMockUri => fundingsApiUri.IsMockUri();

        /// <inheritdoc/>
        public string OriginalFundingUri => fundingsApiUri.GetOriginalFundingUri(
                                                        new[] { apiVersionText, channel, "/funding/notifications{0}" },
                                                        numberOfFundingsToRetrieveFromApi,
                                                        fundingStream,
                                                        IsCFSUri);

        /// <inheritdoc/>
        public string ProviderFundingUri => fundingsApiUri.CombineUri(apiVersionText, channel, "/funding/provider/{0}");

        /// <inheritdoc/>
        public string OriginalFundingLookupUri => fundingsApiUri.CombineUri(apiVersionText, channel, "/funding/byId/{0}");

        /// <inheritdoc/>
        public string OriginalProviderFundingEnrichmentsUri => fundingsApiUri.CombineUri(apiVersionText, channel, "/funding/provider/{0}/fundings");

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReaderInputUriModel"/> class..
        /// </summary>
        /// <param name="fundingsApiUri">Funding (CFS) Api Uri.</param>
        /// <param name="numberOfFundingsToRetrieveFromApi">Number of Fundings to be retrieved from API.</param>
        /// <param name="fundingStream">Funding Stream.</param>
        public FeedReaderInputUriModel(string fundingsApiUri, int numberOfFundingsToRetrieveFromApi, string fundingStream)
        {
            this.fundingsApiUri = fundingsApiUri;
            this.numberOfFundingsToRetrieveFromApi = numberOfFundingsToRetrieveFromApi;
            this.fundingStream = fundingStream;
        }
    }
}
