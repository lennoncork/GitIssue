using System.Collections.Generic;
using System.Text.Json;

namespace GitIssue.Services
{
    /// <summary>
    ///     An issue document returned to a client
    /// </summary>
    public sealed class IssueDocument
    {
        /// <summary>
        ///     Gets the issue key
        /// </summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue type identifier.
        /// </summary>
        public string IssueType { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue title
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue description
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue author
        /// </summary>
        public string Author { get; init; } = string.Empty;

        /// <summary>
        ///     Gets when the issue was created
        /// </summary>
        public string Created { get; init; } = string.Empty;

        /// <summary>
        ///     Gets when the issue was last updated
        /// </summary>
        public string Updated { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue comments
        /// </summary>
        public IReadOnlyList<string> Comments { get; init; } = new List<string>();

        /// <summary>
        ///     Gets every configured field, including custom fields
        /// </summary>
        public IReadOnlyDictionary<string, JsonElement> Fields { get; init; } = new Dictionary<string, JsonElement>();
    }

    /// <summary>
    ///     A short issue description used by list results
    /// </summary>
    public sealed class IssueSummary
    {
        /// <summary>
        ///     Gets the issue key
        /// </summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue title
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the issue description
        /// </summary>
        public string Description { get; init; } = string.Empty;
    }

    /// <summary>
    ///     One page of issue summaries
    /// </summary>
    public sealed class IssuePage
    {
        /// <summary>
        ///     Gets the summaries on this page
        /// </summary>
        public IReadOnlyList<IssueSummary> Items { get; init; } = new List<IssueSummary>();

        /// <summary>
        ///     Gets the total number of matching issues
        /// </summary>
        public int Total { get; init; }
    }

    /// <summary>
    ///     A configured issue field
    /// </summary>
    public sealed class FieldDescriptor
    {
        /// <summary>
        ///     Gets the field name
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the value type name
        /// </summary>
        public string ValueType { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the field storage type name
        /// </summary>
        public string FieldType { get; init; } = string.Empty;

        /// <summary>
        ///     Gets a value indicating whether the field is required
        /// </summary>
        public bool Required { get; init; }
    }

    /// <summary>
    ///     The currently tracked issue
    /// </summary>
    public sealed class TrackedIssueDocument
    {
        /// <summary>
        ///     Gets the tracked issue key. <c>None</c> means nothing is tracked
        /// </summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>
        ///     Gets when tracking started
        /// </summary>
        public string Started { get; init; } = string.Empty;
    }

    /// <summary>
    ///     Change-log entries for one issue
    /// </summary>
    public sealed class ChangeEntry
    {
        /// <summary>
        ///     Gets the issue key
        /// </summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the recorded changes
        /// </summary>
        public IReadOnlyList<string> Entries { get; init; } = new List<string>();
    }

    /// <summary>
    ///     Structured filters for finding issues
    /// </summary>
    public sealed class IssueQuery
    {
        /// <summary>
        ///     Gets text that must appear in the title
        /// </summary>
        public string? TitleContains { get; init; }

        /// <summary>
        ///     Gets text that must appear in the description
        /// </summary>
        public string? DescriptionContains { get; init; }

        /// <summary>
        ///     Gets the field that must be present
        /// </summary>
        public string? Field { get; init; }

        /// <summary>
        ///     Gets the value the field must equal, or contain when the field is an array
        /// </summary>
        public string? FieldEquals { get; init; }

        /// <summary>
        ///     Gets the maximum number of matches
        /// </summary>
        public int Limit { get; init; } = 50;
    }
}
