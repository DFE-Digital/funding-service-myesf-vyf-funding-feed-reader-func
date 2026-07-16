using Pds_azurefunction_fundingfeedreader.Attributes;

namespace Pds_azurefunction_fundingfeedreader.Enums
{
    /// <summary>
    /// Enum to capture the trigger type for the feed reader.
    /// </summary>
    public enum RunTrigger
    {
        /// <summary>
        /// Default enum value.
        /// </summary>
        None = 0,

        /// <summary>
        /// To denote feed reader triggered by Service Bus message.
        /// </summary>
        [DisplayText("Service bus message")]
        ServiceBus = 1,

        /// <summary>
        /// To denote feed reader triggered by HTTP request.
        /// </summary>
        [DisplayText("Manual request")]
        Http = 2,

        /// <summary>
        /// To denote feed reader triggered by Azure timer.
        /// </summary>
        [DisplayText("Auto pull")]
        Timer = 3
    }
}
