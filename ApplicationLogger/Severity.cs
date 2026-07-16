namespace ApplicationLogger
{
    /// <summary>
    /// Severity of logging information.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// provide more verbose information.
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// Useful application information.
        /// </summary>
        Information = 1,

        /// <summary>
        /// application warning.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Application error.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Critical application error.
        /// </summary>
        Critical = 4
    }
}