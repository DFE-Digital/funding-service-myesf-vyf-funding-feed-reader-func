namespace Domain.Messages
{
    /// <summary>
    /// Service bus queue message stating a new or updated version of funding has been added.
    /// </summary>
    public class FundingAddedMessage
    {
        /// <summary>
        /// Gets or sets the funding id.
        /// </summary>
        public string FundingId { get; set; }

        /// <summary>
        /// Gets or sets the provider funding id.
        /// </summary>
        public string ProviderFundingId { get; set; }

        /// <summary>
        /// Gets or sets the FundingStreamCode for the funding.
        /// </summary>
        public string FundingStreamCode { get; set; }

        /// <summary>
        /// Gets or sets provider type (e.g. School, Academy, Special School) - not enumerated as this isn't controlled by CFS, but passed through from the Provider info (GIAS).
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// Gets or sets provider sub type (e.g. Academy special converter) - not enumerated as this isn't controlled by CFS, but passed through from the Provider info (GIAS).
        /// </summary>
        public string ProviderSubType { get; set; }

        /// <summary>
        /// Gets or sets the Ukprn for the funding.
        /// </summary>
        public string Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the FundingPeriodCode for the funding.
        /// </summary>
        public string FundingPeriodCode { get; set; }

        /// <summary>
        /// Gets or sets the CutoffDate for the funding.
        /// </summary>
        public string CutoffDate { get; set; }

        /// <summary>
        /// Gets or sets the local authority name.
        /// </summary>
        public string LAName { get; set; }

        /// <summary>
        /// Gets or sets the local authority code.
        /// </summary>
        public string LACode { get; set; }
    }
}