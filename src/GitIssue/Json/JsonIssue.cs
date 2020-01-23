using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues;
using Newtonsoft.Json.Linq;

namespace GitIssue.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonIssue : Issue, IJsonIssue
    {
        protected readonly Dictionary<FieldKey, IField> fields;
        protected readonly HashSet<FieldKey> modifiedFields;
        protected readonly FileFieldKeyProvider keyProvider;

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonIssue"/> class
        /// </summary>
        /// <param name="root"></param>
        public JsonIssue(IssueRoot root) : base(root)
        {
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonIssue"/> class
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <param name="fields"></param>
        public JsonIssue(IssueRoot issueRoot, IDictionary<FieldKey, FieldInfo> fields) : base(issueRoot)
        {
            this.Root = issueRoot;
            this.fields = fields.ToDictionary(f =>
                    f.Key,
                f => f.Value.CreateField(this, f.Key));
            this.modifiedFields = new HashSet<FieldKey>();
            this.keyProvider = new FileFieldKeyProvider();
        }

        /// <summary>
        /// Gets the Json path for the issue
        /// </summary>
        public string Json => JsonIssue.GetJsonFile(this.Root);

        /// <inheritdoc/>
        public override IEnumerable<FieldKey> Keys => this.fields.Keys;

        /// <inheritdoc/>
        public override IEnumerable<IField> Values => this.fields.Values;

        /// <inheritdoc/>
        public override int Count => this.fields.Count;

        /// <inheritdoc/>
        public override IField this[FieldKey key] => this.fields[key];

        /// <summary>
        /// Gets the JSON file name
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string GetJsonFile(IssueRoot root) => Path.Combine(root.IssuePath, $"{Path.GetFileName(root.Key.ToString())}.json");

        /// <inheritdoc/>
        public override IFieldProvider GetField(string key = null)
            => this.GetField(this.keyProvider.FromString(key));

        /// <inheritdoc/>
        public override IFieldFactory SetField(string key = null)
            => this.SetField(this.keyProvider.FromString(key));

        /// <inheritdoc/>
        public override IFieldProvider GetField(FieldKey key) => new FieldProvider(this, key, () =>
        {
            this.fields.TryGetValue(key, out IField field);
            return field;
        });

        /// <inheritdoc/>
        public override IFieldFactory SetField(FieldKey key) => new FieldFactory(this, key, () =>
        {
            this.fields.TryGetValue(key, out IField field);
            return field;
        });

        /// <inheritdoc/>
        public override async Task<bool> SaveAsync()
        {
            // Make sure the issue root exists
            if (Directory.Exists(this.Root.IssuePath) == false)
                Directory.CreateDirectory(this.Root.IssuePath);

            // Set the created and updated dates
            if (this.Created == DateTime.MinValue)
                this.Created = DateTime.Now;
            this.Updated = DateTime.Now;

            // Save as Json
            await this.SaveAsJsonAsync(this.Json);

            // Success
            return true;
        }

        /// <summary>
        /// Reads an issue from disk
        /// </summary>
        /// <param name="issueRoot">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns></returns>
        public static async Task<IIssue> ReadAsync(IssueRoot issueRoot, IDictionary<FieldKey, FieldInfo> fields)
        {
            if (Directory.Exists(issueRoot.IssuePath) == false)
                return null;

            JsonIssue issue = new JsonIssue(issueRoot, fields);
            foreach (var key in fields.Keys)
            {
                var valueField = await fields[key].ReadFieldAsync(issue, key);
                issue.fields[key] = valueField;
            }

            return issue;
        }

        /// <summary>
        /// Deletes an issue and all it's fields from disk. 
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <returns></returns>
        public static Task DeleteAsync(IssueRoot issueRoot)
        {
            // Issue should exist before attempting to delete
            if (Directory.Exists(issueRoot.IssuePath) == false)
                throw new IssueNotFoundException($"The issue path {issueRoot.IssuePath} does not exist");

            // Delete the json file if it exists
            if(File.Exists(JsonIssue.GetJsonFile(issueRoot)))
                File.Delete(JsonIssue.GetJsonFile(issueRoot));

            // Delete the directory if empty
            if (!Directory.GetFileSystemEntries(issueRoot.IssuePath).Any())
                Directory.Delete(issueRoot.IssuePath);

            // Success
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override bool ContainsKey(FieldKey key) => this.fields.ContainsKey(key);

        /// <inheritdoc/>
        public override bool TryGetValue(FieldKey key, [MaybeNullWhen(false)] out IField value)
        {
            if (this.fields.TryGetValue(key, out IField field))
            {
                value = field;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public override IEnumerator<KeyValuePair<FieldKey, IField>> GetEnumerator()
        {
            foreach (var kvp in this.fields)
            {
                yield return new KeyValuePair<FieldKey, IField>(kvp.Key, kvp.Value);
            }
        }

        /// <inheritdoc/>
        public JObject ToJson()
        {
            JObject json = new JObject();
            foreach (var kvp in fields)
            {
                if (kvp.Value is IJsonField field)
                {
                    json[kvp.Key.ToString()] = field.ToJson();
                }
            }
            return json;
        }
    }
}
