namespace Domain
{
    /// <summary>
    /// Class to enrich the Provider funding.
    /// </summary>
    public class ProviderFundingDetails
    {
        /// <summary>
        /// Gets or sets the schema version number.
        /// </summary>
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of the template (e.g. this is Version 2 of PE and sport template).
        /// </summary>
        public string TemplateVersion { get; set; }

        /// <summary>
        /// Gets or sets the date the funding was published by a business user.
        /// </summary>
        public string StatusChangedDate { get; set; }
    }
}
