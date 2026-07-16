namespace Pds_azurefunction_fundingfeedreader.Models
{
    /// <summary>
    /// Service bus queue message asking the feed reader to run.
    /// </summary>
    public class ServiceBusInputMessage
    {
        /// <summary>
        /// Gets or sets the audit id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the funding streams.
        /// </summary>
        public string FundingStreams { get; set; }

        /// <summary>
        /// Gets or sets the funding periods.
        /// </summary>
        public string FundingPeriods { get; set; }

        /// <summary>
        /// Gets or sets the scenario id.
        /// </summary>
        public string ScenarioIds { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        public string StartDateTime { get; set; }
    }
}