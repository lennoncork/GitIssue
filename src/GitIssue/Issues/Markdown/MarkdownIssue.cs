using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Validation;

namespace GitIssue.Issues.Markdown
{
    /// <summary>
    ///     An issue stored as one Markdown document
    /// </summary>
    public class MarkdownIssue : Json.JsonIssue
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkdownIssue" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        public MarkdownIssue(IssueRoot root)
            : base(root)
        {
            this.Schema = new Dictionary<FieldKey, FieldInfo>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkdownIssue" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the issue fields</param>
        public MarkdownIssue(IssueRoot root, IDictionary<FieldKey, FieldInfo> fields)
            : base(root, fields)
        {
            this.Schema = fields;
        }

        /// <summary>
        ///     Gets the Markdown path for the issue
        /// </summary>
        public string Markdown => MarkdownIssue.GetMarkdownFile(this.Root);

        /// <summary>
        ///     Gets the schema used to read and write the issue
        /// </summary>
        public IDictionary<FieldKey, FieldInfo> Schema { get; }

        /// <summary>
        ///     Deletes an issue document from disk
        /// </summary>
        /// <param name="issueRoot">the issue root</param>
        /// <returns>true when the issue is deleted</returns>
        public static new async Task<bool> DeleteAsync(IssueRoot issueRoot)
        {
            if (!Directory.Exists(issueRoot.IssuePath))
            {
                throw new IssueNotFoundException($"The issue path {issueRoot.IssuePath} does not exist");
            }

            string markdown = MarkdownIssue.GetMarkdownFile(issueRoot);
            if (global::System.IO.File.Exists(markdown))
            {
                global::System.IO.File.Delete(markdown);
            }

            string json = GitIssue.Issues.Json.JsonIssue.GetJsonFile(issueRoot);
            if (global::System.IO.File.Exists(json))
            {
                global::System.IO.File.Delete(json);
            }

            if (Directory.Exists(issueRoot.IssuePath))
            {
                Directory.Delete(issueRoot.IssuePath, true);
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        ///     Gets the Markdown file name
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <returns>the Markdown file path</returns>
        public static string GetMarkdownFile(IssueRoot root)
        {
            return Path.Combine(root.IssuePath, $"{Path.GetFileName(root.Key.ToString())}.md");
        }

        /// <summary>
        ///     Reads an issue from disk
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns>the issue, or null when the issue directory does not exist</returns>
        public static new async Task<IIssue?> ReadAsync(IssueRoot root, IDictionary<FieldKey, FieldInfo> fields)
        {
            if (!Directory.Exists(root.IssuePath))
            {
                return null;
            }

            string path = MarkdownIssue.GetMarkdownFile(root);
            if (!global::System.IO.File.Exists(path))
            {
                return null;
            }

            MarkdownIssue issue = new MarkdownIssue(root, fields);
            string text = await global::System.IO.File.ReadAllTextAsync(path);
            issue.Apply(MarkdownIssueFormat.Read(text, root.Key, fields));
            return issue;
        }

        /// <summary>
        ///     Applies parsed Markdown values onto this issue
        /// </summary>
        /// <param name="read">the parsed document</param>
        public void Apply(MarkdownRead read)
        {
            foreach (KeyValuePair<FieldKey, object?> value in read.Values)
            {
                if (!this.fields.TryGetValue(value.Key, out IField? field) || (value.Value == null))
                {
                    continue;
                }

                if ((value.Value is object[] items) && (field is IArrayField arrayField))
                {
                    arrayField.Values = items;
                }
                else if (field is IValueField valueField)
                {
                    valueField.Value = value.Value;
                }

                this.modifiedFields.Add(value.Key);
            }
        }

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            if (this.Created == System.DateTime.MinValue)
            {
                this.Created = System.DateTime.Now;
            }

            this.Updated = System.DateTime.Now;
            this.EnsureValid();

            if (!Directory.Exists(this.Root.IssuePath))
            {
                Directory.CreateDirectory(this.Root.IssuePath);
            }

            await Paths.WriteAllTextAsync(this.Markdown, MarkdownIssueFormat.Write(this, this.Schema));
            return true;
        }
    }
}
