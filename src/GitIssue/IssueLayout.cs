namespace GitIssue
{
    /// <summary>
    ///     How an issue type chooses its files and folders.
    /// </summary>
    public enum IssueLayout
    {
        /// <summary>
        ///     New issues use the key template. Existing files are still found after they move.
        /// </summary>
        Template,

        /// <summary>
        ///     The Markdown or JSON file can live anywhere under the type directory.
        /// </summary>
        Free,
    }
}
