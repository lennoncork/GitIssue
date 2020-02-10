using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Keys;
using Newtonsoft.Json.Linq;

namespace GitIssue.Json
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
        /// <param name="root"></param>
        public JsonIssue(IssueRoot root) : base(root)
        {
        }

        /// <summary>
        ///     Initializes a new instance of a <see cref="JsonIssue" /> class
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <param name="fields"></param>
        public JsonIssue(IssueRoot issueRoot, IDictionary<FieldKey, FieldInfo> fields) : base(issueRoot)
        {
            Root = issueRoot;
            this.fields = fields.ToDictionary(f =>
                    f.Key,
                f => f.Value.CreateField(this, f.Key));
            modifiedFields = new HashSet<FieldKey>();
            keyProvider = new FileFieldKeyProvider();
        }

        /// <summary>
        ///     Gets the Json path for the issue
        /// </summary>
        public string Json => GetJsonFile(Root);

        /// <inheritdoc />
        public override IEnumerable<FieldKey> Keys => fields.Keys;

        /// <inheritdoc />
        public override IEnumerable<IField> Values => fields.Values;

        /// <inheritdoc />
        public override int Count => fields.Count;

        /// <inheritdoc />
        public override IField this[FieldKey key] => fields[key];

        /// <inheritdoc />
        public override IFieldProvider GetField(string key = null)
        {
            return GetField(keyProvider.FromString(key));
        }

        /// <inheritdoc />
        public override IFieldFactory SetField(string key = null)
        {
            return SetField(keyProvider.FromString(key));
        }

        /// <inheritdoc />
        public override IFieldProvider GetField(FieldKey key)
        {
            return new FieldProvider(this, key, () =>
            {
                fields.TryGetValue(key, out var field);
                return field;
            });
        }

        /// <inheritdoc />
        public override IFieldFactory SetField(FieldKey key)
        {
            return new FieldFactory(this, key, () =>
            {
                fields.TryGetValue(key, out var field);
                return field;
            });
        }

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            // Make sure the issue root exists
            if (Directory.Exists(Root.IssuePath) == false)
                Directory.CreateDirectory(Root.IssuePath);

            // Set the created and updated dates
            if (Created == DateTime.MinValue)
                Created = DateTime.Now;
            Updated = DateTime.Now;

            // Save as Json
            await this.SaveAsJsonAsync(Json);

            // Success
            return true;
        }

        /// <inheritdoc />
        public override bool ContainsKey(FieldKey key)
        {
            return fields.ContainsKey(key);
        }

        /// <inheritdoc />
        public override bool TryGetValue(FieldKey key, [MaybeNullWhen(false)] out IField value)
        {
            if (fields.TryGetValue(key, out var field))
            {
                value = field;
                return true;
            }

            value = null;
            return false;
        }

        /// <inheritdoc />
        public override IEnumerator<KeyValuePair<FieldKey, IField>> GetEnumerator()
        {
            foreach (var kvp in fields) yield return new KeyValuePair<FieldKey, IField>(kvp.Key, kvp.Value);
        }

        /// <inheritdoc />
        public JObject ToJson()
        {
            var json = new JObject();
            foreach (var kvp in fields)
                if (kvp.Value is IJsonField field)
                    json[kvp.Key.ToString()] = field.ToJson();
            return json;
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
        /// <param name="issueRoot">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns></returns>
        public static async Task<IIssue> ReadAsync(IssueRoot issueRoot, IDictionary<FieldKey, FieldInfo> fields)
        {
            if (Directory.Exists(issueRoot.IssuePath) == false)
                return null;

            var issue = new JsonIssue(issueRoot, fields);
            foreach (var key in fields.Keys)
            {
                var valueField = await fields[key].ReadFieldAsync(issue, key);
                issue.fields[key] = valueField;
            }

            return issue;
        }

        /// <summary>
        ///     Deletes an issue and all it's fields from disk.
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <returns></returns>
        public static Task DeleteAsync(IssueRoot issueRoot)
        {
            // Issue should exist before attempting to delete
            if (Directory.Exists(issueRoot.IssuePath) == false)
                throw new IssueNotFoundException($"The issue path {issueRoot.IssuePath} does not exist");

            // Delete the json file if it exists
            if (File.Exists(GetJsonFile(issueRoot)))
                File.Delete(GetJsonFile(issueRoot));

            // Delete the directory if empty
            if (!Directory.GetFileSystemEntries(issueRoot.IssuePath).Any())
                Directory.Delete(issueRoot.IssuePath);

            // Success
            return Task.CompletedTask;
        }
    }
}