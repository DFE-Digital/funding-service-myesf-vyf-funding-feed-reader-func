using CorporateSchema.Version4_00;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Identity.Client;
using System;
using System.Linq;

namespace Pds_azurefunction_fundingfeedreader.Helpers
{
    /// <summary>
    /// A Helper class for Feed Reader Input Uri.
    /// </summary>
    public static class FeedReaderInputUriHelper
    {
        /// <summary>
        /// Check if the funding Uri is a CFS External Api Uri.
        /// </summary>
        /// <param name="fundingsApiUri">Base Funding Api Uri.</param>
        /// <returns>Return true if the funding Uri is CFS external API Uri else return false.</returns>
        public static bool IsCFSUri(this string fundingsApiUri)
        {
            return fundingsApiUri.Contains(".education.", StringComparison.InvariantCultureIgnoreCase) ||
                    fundingsApiUri.Contains("app-t1sb-external-v2.", StringComparison.InvariantCultureIgnoreCase) ||
                    fundingsApiUri.Contains("app-t1dv-external-v2.", StringComparison.InvariantCultureIgnoreCase) ||
                    fundingsApiUri.Contains("app-t1te-external-v2.", StringComparison.InvariantCultureIgnoreCase) ||
                    fundingsApiUri.Contains("app-t1in-external-v2.", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Check if the funding Uri is a CFS External Api Uri.
        /// </summary>
        /// <param name="fundingsApiUri">Base Funding Api Uri.</param>
        /// <returns>Return true if the funding Uri is CFS external API Uri else return false.</returns>
        public static bool IsMockUri(this string fundingsApiUri)
        {
            return fundingsApiUri.Contains("https://pds-mocks-", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Combile Uri.
        /// </summary>
        /// <param name="baseUri">Base Uri.</param>
        /// <param name="pathSegments">Path Segements.</param>
        /// <returns>Combined Uri.</returns>
        public static string CombineUri(this string baseUri, params string[] pathSegments)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            return string.Join("/", new[] { baseUri.TrimEnd('/') }
                .Concat(pathSegments.Where(a => !string.IsNullOrWhiteSpace(a?.Trim('/'))).Select(s => s.Trim('/'))));
        }

        /// <summary>
        /// Populate Original Funding Uri.
        /// </summary>
        /// <param name="fundingsApiUri">Base Funding Api Uri.</param>
        /// <param name="pathSegments">Uri Path Segements.</param>
        /// <param name="numberOfFundingsToRetrieveFromApi">Number Of Fundings To Retrieve From Api.</param>
        /// <param name="fundingStream">Funding Stream Codes.</param>
        /// <param name="isCFSUri">Is CFS Uri.</param>
        /// <returns>Original Funding Uri.</returns>
        public static string GetOriginalFundingUri(
                this string fundingsApiUri,
                string[] pathSegments,
                int numberOfFundingsToRetrieveFromApi,
                string fundingStream,
                bool isCFSUri)
        {
            var uri = fundingsApiUri.CombineUri(pathSegments);

            if (numberOfFundingsToRetrieveFromApi > 0)
            {
                uri = QueryHelpers.AddQueryString(uri, "pageSize", numberOfFundingsToRetrieveFromApi.ToString());
            }

            if (!string.IsNullOrWhiteSpace(fundingStream))
            {
                uri = QueryHelpers.AddQueryString(uri, isCFSUri ? "fundingStreamIds" : "fundingStreamCodes", fundingStream);
            }

            return uri;
        }

        /// <summary>
        /// Populate Original Funding Uri.
        /// </summary>
        /// <param name="originalFundingUri">original Funding Uri.</param>
        /// <param name="fundingPeriod">funding Period.</param>
        /// <param name="scenarioId">scenario Id.</param>
        /// <param name="isCFSUri">Is CFS Uri.</param>
        /// <returns>Original Funding Uri.</returns>
        public static string GetFeedFundingUri(
                this string originalFundingUri,
                string fundingPeriod,
                string scenarioId,
                bool isCFSUri)
        {
            var uri = originalFundingUri;

            if (!string.IsNullOrWhiteSpace(fundingPeriod))
            {
                uri = QueryHelpers.AddQueryString(uri, isCFSUri ? "fundingPeriodIds" : "fundingPeriod", fundingPeriod);
            }

            if (!string.IsNullOrWhiteSpace(scenarioId))
            {
                uri = QueryHelpers.AddQueryString(uri, "scenarioId", scenarioId);
            }

            return uri;
        }
    }
}
