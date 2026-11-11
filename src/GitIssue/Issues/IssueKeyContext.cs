using System;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Values substituted into an issue key template
    /// </summary>
    public sealed class IssueKeyContext
    {
        /// <summary>
        ///     Gets the issue type from configuration
        /// </summary>
        public string Type { get; init; } = "ISSUE";

        /// <summary>
        ///     Gets the issue creation time
        /// </summary>
        public DateTime Created { get; init; } = DateTime.Now;

        /// <summary>
        ///     Gets the sequential index, when the template includes {index}
        /// </summary>
        public int Index { get; init; }

        /// <summary>
        ///     Gets the unique identifier
        /// </summary>
        public string Uid { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue name or title
        /// </summary>
        public string Name { get; init; } = "Untitled";
    }
}
