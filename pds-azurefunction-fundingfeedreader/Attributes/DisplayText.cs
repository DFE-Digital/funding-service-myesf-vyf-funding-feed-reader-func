using System;

namespace Pds_azurefunction_fundingfeedreader.Attributes
{
    /// <summary>
    /// Attribute class to assign readable string representations to enums.
    /// </summary>
    public class DisplayText : Attribute
    {
        private string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayText"/> class.
        /// </summary>
        /// <param name="displayText">Readable string representation to be applied to enum.</param>
        public DisplayText(string displayText)
        {
            _text = displayText;
        }

        /// <summary>
        /// Gets or sets the Text property which will contain the readable string representation of enum.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }
}
