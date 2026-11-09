using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Services;
using ModelContextProtocol.Server;

namespace GitIssue.Services.Mcp.Tools
{
    /// <summary>
    ///     MCP tools for issues stored in the working tree
    /// </summary>
    [McpServerToolType]
    public sealed class IssueTools
    {
        private readonly IssueSession session;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueTools" /> class
        /// </summary>
        /// <param name="session">the issue session</param>
        public IssueTools(IssueSession session)
        {
            this.session = session;
        }

        /// <summary>
        ///     Lists issues
        /// </summary>
        /// <param name="limit">the maximum number of issues</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the issue summaries</returns>
        [McpServerTool(Name = "list_issues")]
        [Description("Lists issue keys, titles, and descriptions. A feature is an issue whose configured fields, such as Labels or Type, say so.")]
        public Task<IReadOnlyList<IssueSummary>> ListIssues(
            [Description("Maximum number of issues to return, from 1 to 200.")] int limit = 50,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().ListIssuesAsync(limit, cancellationToken);
        }

        /// <summary>
        ///     Gets one issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the issue document</returns>
        [McpServerTool(Name = "get_issue")]
        [Description("Gets one issue, including comments and every configured field.")]
        public Task<IssueDocument> GetIssue(
            [Description("The issue key.")] string key,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().GetIssueAsync(key, cancellationToken);
        }

        /// <summary>
        ///     Finds issues
        /// </summary>
        /// <param name="titleContains">text that must appear in the title</param>
        /// <param name="descriptionContains">text that must appear in the description</param>
        /// <param name="field">a configured field that must be present</param>
        /// <param name="fieldEquals">the value that field must equal, or contain when it is an array</param>
        /// <param name="limit">the maximum number of matches</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the matching documents</returns>
        [McpServerTool(Name = "find_issues")]
        [Description("Finds issues by title, description, or an exact field value. Array fields match when they contain the value. This does not evaluate C#.")]
        public Task<IReadOnlyList<IssueDocument>> FindIssues(
            [Description("Case-insensitive text that must appear in the title.")] string? titleContains = null,
            [Description("Case-insensitive text that must appear in the description.")] string? descriptionContains = null,
            [Description("Configured field name that must be present.")] string? field = null,
            [Description("Value the field must equal. For an array field, a value the array must contain.")] string? fieldEquals = null,
            [Description("Maximum number of matches, from 1 to 200.")] int limit = 50,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().FindIssuesAsync(
                new IssueQuery
                {
                    TitleContains = titleContains,
                    DescriptionContains = descriptionContains,
                    Field = field,
                    FieldEquals = fieldEquals,
                    Limit = limit,
                },
                cancellationToken);
        }

        /// <summary>
        ///     Lists configured fields
        /// </summary>
        /// <returns>the field schema</returns>
        [McpServerTool(Name = "list_fields")]
        [Description("Lists the configured issue fields. Call this before create_issue or update_issue. Features are represented by these fields, not by a separate record type.")]
        public IReadOnlyList<FieldDescriptor> ListFields()
        {
            return this.session.Open().ListFields();
        }

        /// <summary>
        ///     Gets the tracked issue
        /// </summary>
        /// <returns>the tracked issue</returns>
        [McpServerTool(Name = "get_tracked_issue")]
        [Description("Gets the tracked issue key. The key is None when nothing is tracked.")]
        public TrackedIssueDocument GetTrackedIssue()
        {
            return this.session.Open().GetTracked();
        }

        /// <summary>
        ///     Gets the change log
        /// </summary>
        /// <returns>the change log</returns>
        [McpServerTool(Name = "get_changes")]
        [Description("Gets the in-memory issue change log. Creating an issue records a change. These tools do not commit.")]
        public IReadOnlyList<ChangeEntry> GetChanges()
        {
            return this.session.Open().GetChanges();
        }

        /// <summary>
        ///     Creates an issue
        /// </summary>
        /// <param name="title">the title</param>
        /// <param name="description">the description</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the created document</returns>
        [McpServerTool(Name = "create_issue")]
        [Description("Creates an issue in the working tree and does not commit it.")]
        public Task<IssueDocument> CreateIssue(
            [Description("The issue title.")] string title,
            [Description("The issue description.")] string? description = null,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().CreateIssueAsync(title, description, cancellationToken);
        }

        /// <summary>
        ///     Updates one field
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="field">the field name</param>
        /// <param name="value">the field value</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the updated document</returns>
        [McpServerTool(Name = "update_issue")]
        [Description("Updates one configured field and does not commit. Array fields use CLI syntax: +value adds and -value removes. Key is read-only.")]
        public Task<IssueDocument> UpdateIssue(
            [Description("The issue key.")] string key,
            [Description("The configured field name.")] string field,
            [Description("The new value, or +value / -value for an array field.")] string value,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().UpdateIssueAsync(key, field, value, cancellationToken);
        }

        /// <summary>
        ///     Adds a comment
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="comment">the comment</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the updated document</returns>
        [McpServerTool(Name = "add_comment")]
        [Description("Appends a comment to an issue and does not commit.")]
        public Task<IssueDocument> AddComment(
            [Description("The issue key.")] string key,
            [Description("The comment text.")] string comment,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().AddCommentAsync(key, comment, cancellationToken);
        }

        /// <summary>
        ///     Tracks an issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the tracked issue</returns>
        [McpServerTool(Name = "track_issue")]
        [Description("Tracks an existing issue.")]
        public Task<TrackedIssueDocument> TrackIssue(
            [Description("The issue key.")] string key,
            CancellationToken cancellationToken = default)
        {
            return this.session.Open().TrackIssueAsync(key, cancellationToken);
        }
    }
}
