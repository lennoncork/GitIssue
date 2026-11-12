using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Exceptionable;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Issues;

namespace GitIssue.Services
{
    /// <summary>
    ///     Issue operations shared by the MCP server and the web server
    /// </summary>
    public sealed class IssueWorkspace : IDisposable
    {
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);

        private readonly IIssueManager manager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueWorkspace" /> class
        /// </summary>
        /// <param name="manager">the issue manager</param>
        public IssueWorkspace(IIssueManager manager)
        {
            this.manager = manager;
        }

        private static bool tryResolveField(IIssue issue, string field, out FieldKey key)
        {
            foreach (FieldKey candidate in issue.Keys)
            {
                if (string.Equals(candidate.ToString(), field, StringComparison.OrdinalIgnoreCase))
                {
                    key = candidate;
                    return true;
                }
            }

            key = FieldKey.None;
            return false;
        }

        private static int normalizeLimit(int limit)
        {
            if (limit < 1)
            {
                throw new IssueOperationException("Limit must be at least 1.");
            }

            return Math.Min(limit, 200);
        }

        private static bool matches(IssueDocument document, IssueQuery query)
        {
            if (!IssueWorkspace.contains(document.Title, query.TitleContains) ||
                !IssueWorkspace.contains(document.Description, query.DescriptionContains))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(query.Field))
            {
                return true;
            }

            if (!IssueWorkspace.tryGetField(document, query.Field, out JsonElement field))
            {
                return false;
            }

            if (string.IsNullOrEmpty(query.FieldEquals))
            {
                return true;
            }

            return IssueWorkspace.equals(field, query.FieldEquals);
        }

        private static bool contains(string text, string? expected)
        {
            if (string.IsNullOrEmpty(expected))
            {
                return true;
            }

            return text.Contains(expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool tryGetField(IssueDocument document, string name, out JsonElement field)
        {
            foreach (KeyValuePair<string, JsonElement> entry in document.Fields)
            {
                if (string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    field = entry.Value;
                    return true;
                }
            }

            field = default;
            return false;
        }

        private static bool equals(JsonElement element, string expected)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return string.Equals(element.GetString(), expected, StringComparison.OrdinalIgnoreCase);
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    if (IssueWorkspace.equals(item, expected))
                    {
                        return true;
                    }
                }

                return false;
            }

            return string.Equals(element.ToString(), expected, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.manager.Dispose();
            this.gate.Dispose();
        }

        /// <summary>
        ///     Lists issues up to the requested limit
        /// </summary>
        /// <param name="limit">the maximum number of issues</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the matching summaries</returns>
        public Task<IReadOnlyList<IssueSummary>> ListIssuesAsync(int limit, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.listAsync(limit, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Searches and pages through all issue summaries
        /// </summary>
        /// <param name="search">text to match in the key, title, or description</param>
        /// <param name="page">the one-based page number</param>
        /// <param name="pageSize">the number of items on each page</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the requested page and total match count</returns>
        [Throws<IssueOperationException>]
        public Task<IssuePage> SearchIssuesAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
        {
            if ((page < 1) || (pageSize < 1) || (pageSize > 200))
            {
                throw new IssueOperationException("Page must be at least 1 and page size must be between 1 and 200.");
            }

            return this.withLockAsync(
                () => this.searchAsync(search, page, pageSize, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Gets one issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the issue document</returns>
        [Throws<IssueOperationException>]
        public Task<IssueDocument> GetIssueAsync(string key, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.getAsync(key, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Finds issues with structured filters
        /// </summary>
        /// <param name="query">the filters</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the matching documents</returns>
        public Task<IReadOnlyList<IssueDocument>> FindIssuesAsync(IssueQuery query, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.findAsync(query, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Lists the configured fields
        /// </summary>
        /// <returns>the field schema</returns>
        public IReadOnlyList<FieldDescriptor> ListFields(string? issueType = null)
        {
            return this.withLock(() => this.listFields(issueType));
        }

        /// <summary>
        ///     Lists the configured issue type identifiers.
        /// </summary>
        /// <returns>the configured types, or the default type for a legacy schema</returns>
        public IReadOnlyList<string> ListIssueTypes()
        {
            return this.withLock<IReadOnlyList<string>>(() =>
                this.manager.Configuration.Issues.Count == 0
                    ? new[] { this.manager.Configuration.IssueType }
                    : this.manager.Configuration.Issues.Select(issue => issue.Name).ToArray());
        }

        /// <summary>
        ///     Gets the tracked issue
        /// </summary>
        /// <returns>the tracked issue</returns>
        public TrackedIssueDocument GetTracked()
        {
            return this.withLock(this.getTracked);
        }

        /// <summary>
        ///     Gets the change log
        /// </summary>
        /// <returns>the recorded changes</returns>
        public IReadOnlyList<ChangeEntry> GetChanges()
        {
            return this.withLock(this.getChanges);
        }

        /// <summary>
        ///     Creates an issue without committing it
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <param name="description">the issue description</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the created document</returns>
        [Throws<IssueOperationException>]
        public Task<IssueDocument> CreateIssueAsync(string title, string? description, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.createAsync(title, description, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Creates an issue of a specified type with initial field values.
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <param name="description">the issue description</param>
        /// <param name="issueType">the issue type, or null for the default</param>
        /// <param name="fields">initial field values</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the created issue document</returns>
        public Task<IssueDocument> CreateIssueAsync(string title, string? description, string? issueType, IReadOnlyDictionary<string, string>? fields, CancellationToken cancellationToken)
        {
            return this.withLockAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new IssueOperationException("A title is required.");
                }

                string type = issueType ?? this.manager.Configuration.IssueType;
                Dictionary<FieldKey, FieldInfo> schema = this.manager.Configuration.GetFields(type);
                List<IField> initial = new List<IField>();
                if (fields != null)
                {
                    foreach (KeyValuePair<string, string> pair in fields)
                    {
                        FieldKey key = FieldKey.Create(pair.Key);
                        if (pair.Key is "Key" or "IssueType" || !schema.TryGetValue(key, out FieldInfo? info))
                        {
                            throw new IssueOperationException($"Field \"{pair.Key}\" cannot be supplied for issue type \"{type}\".");
                        }

                        IField field = info.CreateField(null!, key);
                        if (!field.Update(pair.Value))
                        {
                            throw new IssueOperationException($"Value \"{pair.Value}\" is not valid for field \"{pair.Key}\".");
                        }

                        initial.Add(field);
                    }
                }

                IIssue issue = await this.manager.CreateAsync(title.Trim(), description ?? string.Empty, type, initial);
                return IssueDocumentMapper.FromIssue(issue);
            }, cancellationToken);
        }

        /// <summary>
        ///     Updates one field without committing the issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="field">the field name</param>
        /// <param name="value">the field value, using the CLI field syntax</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the updated document</returns>
        [Throws<IssueOperationException>]
        public Task<IssueDocument> UpdateIssueAsync(string key, string field, string value, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.updateAsync(key, field, value, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Adds a comment without committing the issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="comment">the comment text</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the updated document</returns>
        [Throws<IssueOperationException>]
        public Task<IssueDocument> AddCommentAsync(string key, string comment, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.addCommentAsync(key, comment, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        ///     Tracks an existing issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the tracked issue</returns>
        [Throws<IssueOperationException>]
        public Task<TrackedIssueDocument> TrackIssueAsync(string key, CancellationToken cancellationToken)
        {
            return this.withLockAsync(
                () => this.trackAsync(key, cancellationToken),
                cancellationToken);
        }

        private async Task<IReadOnlyList<IssueSummary>> listAsync(int limit, CancellationToken cancellationToken)
        {
            int take = IssueWorkspace.normalizeLimit(limit);
            List<IssueSummary> summaries = new List<IssueSummary>();
            await foreach (IIssue issue in this.manager.FindAsync(candidate => true).WithCancellation(cancellationToken))
            {
                summaries.Add(IssueDocumentMapper.ToSummary(IssueDocumentMapper.FromIssue(issue)));
                if (summaries.Count >= take)
                {
                    break;
                }
            }

            return summaries;
        }

        private async Task<IssuePage> searchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<IssueSummary> matches = new List<IssueSummary>();
            string text = search?.Trim() ?? string.Empty;
            await foreach (IIssue issue in this.manager.FindAsync(candidate => true).WithCancellation(cancellationToken))
            {
                IssueSummary summary = IssueDocumentMapper.ToSummary(IssueDocumentMapper.FromIssue(issue));
                if (summary.Key.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    summary.Title.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    summary.Description.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(summary);
                }
            }

            matches.Sort((left, right) => StringComparer.Ordinal.Compare(left.Key, right.Key));
            long offset = ((long)page - 1) * pageSize;
            return new IssuePage
            {
                Items = offset >= matches.Count ? new List<IssueSummary>() : matches.Skip((int)offset).Take(pageSize).ToList(),
                Total = matches.Count,
            };
        }

        private async Task<IssueDocument> getAsync(string key, CancellationToken cancellationToken)
        {
            return IssueDocumentMapper.FromIssue(await this.requireAsync(key, cancellationToken));
        }

        private async Task<IReadOnlyList<IssueDocument>> findAsync(IssueQuery query, CancellationToken cancellationToken)
        {
            int take = IssueWorkspace.normalizeLimit(query.Limit);
            List<IssueDocument> documents = new List<IssueDocument>();
            await foreach (IIssue issue in this.manager.FindAsync(candidate => true).WithCancellation(cancellationToken))
            {
                IssueDocument document = IssueDocumentMapper.FromIssue(issue);
                if (!IssueWorkspace.matches(document, query))
                {
                    continue;
                }

                documents.Add(document);
                if (documents.Count >= take)
                {
                    break;
                }
            }

            return documents;
        }

        private IReadOnlyList<FieldDescriptor> listFields(string? issueType)
        {
            List<FieldDescriptor> fields = new List<FieldDescriptor>();
            foreach (KeyValuePair<FieldKey, FieldInfo> field in this.manager.Configuration.GetFields(issueType ?? this.manager.Configuration.IssueType))
            {
                fields.Add(new FieldDescriptor
                {
                    Name = field.Key.ToString(),
                    ValueType = field.Value.ValueType.ToString() ?? string.Empty,
                    FieldType = field.Value.FieldType.ToString() ?? string.Empty,
                    Required = field.Value.Required,
                });
            }

            return fields;
        }

        private TrackedIssueDocument getTracked()
        {
            return new TrackedIssueDocument
            {
                Key = this.manager.Tracked.Key.ToString(),
                Started = this.manager.Tracked.Started.ToString("o", CultureInfo.InvariantCulture),
            };
        }

        private IReadOnlyList<ChangeEntry> getChanges()
        {
            List<ChangeEntry> changes = new List<ChangeEntry>();
            foreach (KeyValuePair<IssueKey, List<string>> entry in this.manager.Changes.Log)
            {
                changes.Add(new ChangeEntry
                {
                    Key = entry.Key.ToString(),
                    Entries = entry.Value,
                });
            }

            return changes;
        }

        private async Task<IssueDocument> createAsync(string title, string? description, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new IssueOperationException("A title is required.");
            }

            IIssue issue = await this.manager.CreateAsync(title.Trim(), description ?? string.Empty);
            return IssueDocumentMapper.FromIssue(issue);
        }

        private async Task<IssueDocument> updateAsync(string key, string field, string value, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new IssueOperationException("A field name is required.");
            }

            if (string.Equals(field, "Key", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(field, "IssueType", StringComparison.OrdinalIgnoreCase))
            {
                throw new IssueOperationException($"The {field} field is read-only.");
            }

            IIssue issue = await this.requireAsync(key, cancellationToken);
            if (!IssueWorkspace.tryResolveField(issue, field, out FieldKey fieldKey))
            {
                throw new IssueOperationException($"Field \"{field}\" is not configured.");
            }

            if (!issue[fieldKey].Update(value))
            {
                throw new IssueOperationException($"Value \"{value}\" is not valid for field \"{field}\".");
            }

            await issue.SaveAsync();
            return IssueDocumentMapper.FromIssue(issue);
        }

        private async Task<IssueDocument> addCommentAsync(string key, string comment, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new IssueOperationException("A comment is required.");
            }

            IIssue issue = await this.requireAsync(key, cancellationToken);
            FieldKey comments = FieldKey.Create("Comments");
            if (!issue.TryGetValue(comments, out IField? field) || (field is not IArrayField array))
            {
                throw new IssueOperationException("The Comments field is not configured.");
            }

            if (!array.TryParse(comment, out object? value) || (value is null))
            {
                throw new IssueOperationException("The comment text is not valid.");
            }

            IList values = array;
            values.Add(value);
            await issue.SaveAsync();
            return IssueDocumentMapper.FromIssue(issue);
        }

        private async Task<TrackedIssueDocument> trackAsync(string key, CancellationToken cancellationToken)
        {
            IIssue issue = await this.requireAsync(key, cancellationToken);
            await this.manager.TrackAsync(issue.Key);
            return this.getTracked();
        }

        private async Task<IIssue> requireAsync(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new IssueOperationException("An issue key is required.");
            }

            await foreach (IIssue issue in this.manager.FindAsync(candidate => candidate.Key.ToString() == key).WithCancellation(cancellationToken))
            {
                return issue;
            }

            throw new IssueOperationException($"Issue \"{key}\" was not found.");
        }

        private async Task<T> withLockAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            await this.gate.WaitAsync(cancellationToken);
            try
            {
                return await action();
            }
            finally
            {
                this.gate.Release();
            }
        }

        private T withLock<T>(Func<T> action)
        {
            this.gate.Wait();
            try
            {
                return action();
            }
            finally
            {
                this.gate.Release();
            }
        }
    }
}
