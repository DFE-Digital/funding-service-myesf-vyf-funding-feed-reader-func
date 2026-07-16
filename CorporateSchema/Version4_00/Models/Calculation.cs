using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorporateSchema.Version4_00
{
    /// <summary>
    /// A calculation used to build up a funding line.
    /// </summary>
    public class Calculation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Calculation"/> class.
        /// </summary>
        public Calculation()
        {
            AggregationType = "Sum";
        }

        /// <summary>
        /// Gets or sets the name of the calculation. Used as a description within the model.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the template calculation id (i.e. a way to get to this property in the template).
        /// This value can be the same for multiple calculations within the hierarchy.
        /// This indicates they will return the same value from the output.
        /// It allows input template to link calculations together, so a single calculation implementation will be created instead of multiple depending on the hierarchy.
        /// When templates are versioned, template IDs should be kept the same if they refer to the same thing, otherwise a new, unused ID should be used.
        /// </summary>
        [JsonProperty("templateCalculationId")]
        public uint TemplateCalculationId { get; set; }

        /// <summary>
        /// Gets or sets the value the calculation is resulting in.
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the way the value should show.
        /// </summary>
        [JsonProperty("valueFormat")]
        public string ValueFormat { get; set; }

        /// <summary>
        /// Gets or sets the type of calculation.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets presentation data about how a formula is built up.
        /// </summary>
        [JsonProperty("formulaText")]
        public string FormulaText { get; set; }

        /// <summary>
        /// Gets or sets how the calculation should aggregate.
        /// </summary>
        [JsonProperty("aggregationType")]
        public string AggregationType { get; set; }

        /// <summary>
        /// Gets or sets sub level calculations.
        /// </summary>
        [JsonProperty("calculations", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Calculation> Calculations { get; set; }
    }
}