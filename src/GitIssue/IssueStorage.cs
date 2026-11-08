namespace GitIssue
{
    /// <summary>
    ///     On-disk format used for an issue document
    /// </summary>
    public enum IssueStorage
    {
        /// <summary>
        ///     One JSON document per issue
        /// </summary>
        Json = 0,

        /// <summary>
        ///     One Markdown document per issue
        /// </summary>
        Markdown = 1,
    }
}