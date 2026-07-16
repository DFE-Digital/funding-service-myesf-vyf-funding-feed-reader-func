using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// An interface that represents Feed Reader Input Uri.
    /// </summary>
    public interface IFeedReaderInputUriModel
    {
        /// <summary>
        /// Gets a value indicating whether the base Uri is for CFS External Api.
        /// </summary>
        bool IsCFSUri { get; }

        /// <summary>
        /// Gets a value indicating whether the base Uri is for Mock Api.
        /// </summary>
        bool IsMockUri { get; }

        /// <summary>
        /// Gets Original Funding Uri.
        /// </summary>
        string OriginalFundingUri { get; }

        /// <summary>
        /// Gets Provider Funding Uri.
        /// </summary>
        string ProviderFundingUri { get; }

        /// <summary>
        /// Gets Original Funding Lookup Uri.
        /// </summary>
        string OriginalFundingLookupUri { get; }

        /// <summary>
        /// Gets Original Provider Funding Enrichment Uri.
        /// </summary>
        string OriginalProviderFundingEnrichmentsUri { get; }
    }
}
