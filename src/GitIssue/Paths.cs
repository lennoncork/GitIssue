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
        ///     Gets or sets the config folder name
        /// </summary>
        public static string ConfigFileName { get; set; } = "config";
    }
}