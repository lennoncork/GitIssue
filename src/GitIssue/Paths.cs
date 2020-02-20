namespace GitIssue
{
    /// <summary>
    ///     The paths for the folders
    /// </summary>
    public static class Paths
    {
        /// <summary>
        ///     Gets or sets the name of the git folder
        /// </summary>
        public static string GitFolderName { get; set; } = ".git";

        /// <summary>
        ///     Gets or sets the name of the issues folder
        /// </summary>
        public static string IssueRootFolderName { get; set; } = ".issues";

        /// <summary>
        ///     Gets or sets the config file name
        /// </summary>
        public static string ConfigFileName { get; set; } = "config.json";

        /// <summary>
        ///     Gets or sets the change log file name
        /// </summary>
        public static string ChangeLogFileName { get; set; } = "changes.json";

        /// <summary>
        /// Gets or sets the tracked issue file name
        /// </summary>
        public static string TrackedIssueFileName { get; set; } = "tracking.json";
    }
}