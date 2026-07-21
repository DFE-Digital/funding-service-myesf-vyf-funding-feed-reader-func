namespace Pds_azurefunction_fundingfeedreader.Helpers
{
    /// <summary>
    /// The Url helper class.
    /// </summary>
    public static class UrlHelper
    {
        /// <summary>
        /// Adds the funding stream codes to original funding URI.
        /// </summary>
        /// <param name="originalFundingUri">The original funding URI.</param>
        /// <param name="fundingStream">The funding streams comma separated.</param>
        /// <param name="isCfs">if set to <c>true</c> [is CFS].</param>
        /// <returns>Updated url.</returns>
        public static string AddFundingStreamCodeToOriginalFundingUri(
            this string originalFundingUri,
            string fundingStream,
            bool isCfs)
        {
            if (!string.IsNullOrWhiteSpace(fundingStream))
            {
                if (isCfs)
                {
                    originalFundingUri = $"{originalFundingUri}&fundingStreamIds={fundingStream}";
                }
                else
                {
                    originalFundingUri = AddQuerystringParams(
                        originalFundingUri,
                        "&",
                        "fundingStreamCodes",
                        fundingStream);
                }
            }

            return originalFundingUri;
        }

        /// <summary>
        /// Adds the querystring parameters.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="seperator">The seperator.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The query string parameter.</returns>
        public static string AddQuerystringParams(string inputString, string seperator, string key, string value)
        {
            return !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value) ? $"{inputString}{seperator}{key}={value}" : inputString;
        }
    }
}
