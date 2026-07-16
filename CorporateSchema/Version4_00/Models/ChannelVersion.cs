using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateSchema.Version4_00.Models
{
    /// <summary>
    /// A class representing a channel version.
    /// </summary>
    public class ChannelVersion
    {
        /// <summary>
        /// Gets or sets the type of the Channel.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the version Value of the Channel.
        /// </summary>
        [JsonProperty("value")]
        public int? Value { get; set; }
    }
}
