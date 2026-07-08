using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues.File;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     A JSON backed issue type
    /// </summary>
    public class JsonIssue : Issue, IJsonIssue
    {
        /// <summary>
        ///     The dictionary of fields
        /// </summary>
        protected readonly Dictionary<FieldKey, IField> fields;

        /// <summary>
        ///     The key provider
        /// </summary>
        protected readonly FileFieldKeyProvider keyProvider;

        /// <summary>
        ///     The set of modified fields (that need to be saved)
        /// </summary>
        protected readonly HashSet<FieldKey> modifiedFields;

        /// <summary>
        ///     Initializes a new instance of a <see cref="JsonIssue" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        public JsonIssue(IssueRoot root) : base(root)
        {
            this.fields = new Dictionary<FieldKey, IField>();
            this.modifiedFields = new HashSet<FieldKey>();
            this.keyProvider = new FileFieldKeyProvider();
            this.Key = root.Key;
        }

        /// <summary>
        ///     Initializes a new instance of a <see cref="JsonIssue" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the issue's fields</param>
        public JsonIssue(IssueRoot root, IDictionary<FieldKey, FieldInfo> fields) :
            base(root)
        {
            this.fields = fields.ToDictionary(f => f.Key,
                f => f.Value.CreateField(this, f.Key));
            this.modifiedFields = new HashSet<FieldKey>();
            this.keyProvider = new FileFieldKeyProvider();
            this.Key = root.Key;
        }

        /// <inheritdoc />
        public override int Count => this.fields.Count;

        /// <inheritdoc />
        public override IField this[FieldKey key] => this.fields[key];

        /// <summary>
        ///     Gets the Json path for the issue
        /// </summary>
        public string Json => JsonIssue.GetJsonFile(this.Root);

        /// <inheritdoc />
        public override IEnumerable<FieldKey> Keys => this.fields.Keys;

        /// <inheritdoc />
        public override IEnumerable<IField> Values => this.fields.Values;

        /// <summary>
        ///     Deletes an issue and all it's fields from disk.
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <returns></returns>
        public static Task<bool> DeleteAsync(IssueRoot issueRoot)
        {
            // Issue should exist before attempting to delete
            if (!Directory.Exists(issueRoot.IssuePath))
            {
                throw new IssueNotFoundException($"The issue path {issueRoot.IssuePath} does not exist");
            }

            // Delete the json file if it exists
            if (System.IO.File.Exists(JsonIssue.GetJsonFile(issueRoot)))
            {
                System.IO.File.Delete(JsonIssue.GetJsonFile(issueRoot));
            }

            // Delete the directory if empty
            if (!Directory.GetFileSystemEntries(issueRoot.IssuePath).Any())
            {
                Directory.Delete(issueRoot.IssuePath);
            }

            // Success
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Gets the JSON file name
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string GetJsonFile(IssueRoot root)
        {
            return Path.Combine(root.IssuePath, $"{Path.GetFileName(root.Key.ToString())}.json");
        }

        /// <summary>
        ///     Reads an issue from disk
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns></returns>
        public static async Task<IIssue?> ReadAsync(IssueRoot root,
            IDictionary<FieldKey, FieldInfo> fields)
        {
            if (!Directory.Exists(root.IssuePath))
            {
                return null;
            }

            JsonIssue issue = new JsonIssue(root, fields);
            foreach (FieldKey key in fields.Keys)
            {
                IField valueField = await fields[key].ReadFieldAsync(issue, key);
                issue.fields[key] = valueField;
            }

            return issue;
        }

        /// <inheritdoc />
        public override IEnumerator<KeyValuePair<FieldKey, IField>> GetEnumerator()
        {
            foreach (KeyValuePair<FieldKey, IField> kvp in this.fields)
            {
                yield return new KeyValuePair<FieldKey, IField>(kvp.Key, kvp.Value);
            }
        }

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            // Make sure the issue root exists
            if (!Directory.Exists(this.Root.IssuePath))
            {
                Directory.CreateDirectory(this.Root.IssuePath);
            }

            // Set the created and updated dates
            if (this.Created == DateTime.MinValue)
            {
                this.Created = DateTime.Now;
            }

            this.Updated = DateTime.Now;

            // Save as Json
            await this.SaveAsJsonAsync(this.Json);

            // Success
            return true;
        }

        /// <inheritdoc />
        public override IFieldFactory SetField(string? key = null)
        {
            return this.SetField(this.keyProvider.FromString(key));
        }

        /// <inheritdoc />
        public override IFieldFactory SetField(FieldKey key)
        {
            return new FieldFactory(this, key, () =>
            {
                this.fields.TryGetValue(key, out IField? field);
                return field;
            });
        }

        /// <inheritdoc />
        public JObject ToJson()
        {
            JObject json = new JObject();
            foreach (KeyValuePair<FieldKey, IField> kvp in this.fields)
            {
                if (kvp.Value is IJsonField field)
                {
                    json[kvp.Key.ToString()] = field.ToJson();
                }
            }

            return json;
        }

        /// <inheritdoc />
        public override bool ContainsKey(FieldKey key)
        {
            return this.fields.ContainsKey(key);
        }

        /// <inheritdoc />
        public override bool TryGetValue(FieldKey key, [MaybeNullWhen(false)] out IField value)
        {
            if (this.fields.TryGetValue(key, out IField? field))
            {
                value = field;
                return true;
            }

            value = null!;
            return false;
        }

        /// <inheritdoc />
        public override IFieldProvider GetField(string? key = null)
        {
            return this.GetField(this.keyProvider.FromString(key));
        }

        /// <inheritdoc />
        public override IFieldProvider GetField(FieldKey key)
        {
            return new FieldProvider(this, key, () =>
            {
                this.fields.TryGetValue(key, out IField? field);
                return field;
            });
        }
    }
}