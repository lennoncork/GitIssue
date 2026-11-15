using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Issues;

namespace GitIssue.Syncs
{
    /// <summary>
    ///     Imports issues from a JSON description
    /// </summary>
    public abstract class Importer : IImporter
    {
        private readonly IIssueManager manager;

        /// <summary>
        ///     Initializes an instance of the <see cref="Importer" /> class
        /// </summary>
        /// <param name="manager"></param>
        public Importer(IIssueManager manager)
        {
            this.Root = new SyncRoot(manager.Root, "imports");
            this.manager = manager;
            this.Configuration = new SyncConfiguration();
        }

        /// <summary>
        ///     The configuration of the sync
        /// </summary>
        public SyncConfiguration Configuration { get; set; }

        /// <summary>
        ///     Gets the base path for imported issues
        /// </summary>
        public SyncRoot Root { get; set; }

        /// <inheritdoc />
        public async IAsyncEnumerator<SyncedIssue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(this.Root.ImportPath))
            {
                yield break;
            }

            IEnumerable<string> files = Directory
                .EnumerateFiles(this.Root.ImportPath, "*.import", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return await SyncedIssue.ReadAsync(this, file);
            }
        }

        /// <inheritdoc />
        public abstract Task<bool> Import();

        /// <summary>
        ///     Imports an issue from a JSON object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        protected async Task<bool> Import(JsonObject json)
        {
            string issueType = this.manager.Configuration.IssueType;
            bool mappedType = this.Configuration.Mapping.TryGetValue("IssueType", out string? configuredPath);
            string typePath = mappedType ? configuredPath! : "IssueType";
            JsonNode? typeNode = Importer.selectNode(json, typePath);
            if (typeNode == null && (mappedType || json.ContainsKey("IssueType")))
            {
                throw new InvalidOperationException("The imported IssueType must be a string.");
            }

            if (typeNode != null)
            {
                if (typeNode is not JsonValue typeValue ||
                    !JsonSerialization.TryGetScalar(typeValue, out object? scalar) ||
                    scalar is not string selected)
                {
                    throw new InvalidOperationException("The imported IssueType must be a string.");
                }

                issueType = selected;
            }

            Dictionary<FieldKey, FieldInfo> schema = this.manager.Configuration.GetFields(issueType);
            Dictionary<FieldKey, JsonNode> tokens = new Dictionary<FieldKey, JsonNode>();
            foreach (KeyValuePair<string, string> kvp in this.Configuration.Mapping)
            {
                FieldKey key = FieldKey.Create(kvp.Key);
                if (key == "IssueType")
                {
                    continue;
                }

                JsonNode? token = Importer.selectNode(json, kvp.Value);
                if (token != null && !schema.ContainsKey(key))
                {
                    throw new InvalidOperationException($"Field '{key}' does not exist on issue type '{issueType}'.");
                }

                if (token != null)
                {
                    tokens[key] = token;
                }
            }

            return await this.import(tokens, issueType, schema);
        }

        private static JsonNode? selectNode(JsonNode node, string path)
        {
            JsonNode? current = node;
            foreach (string segment in path.TrimStart('$', '.').Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                int bracket = segment.IndexOf('[');
                string property = bracket >= 0 ? segment.Substring(0, bracket) : segment;
                if (!string.IsNullOrEmpty(property))
                {
                    current = (current as JsonObject)?[property];
                }

                while ((current != null) && (bracket >= 0))
                {
                    int endBracket = segment.IndexOf(']', bracket + 1);
                    if ((endBracket < 0) ||
                        !int.TryParse(segment.Substring(bracket + 1, endBracket - bracket - 1), out int index) ||
                        current is not JsonArray array ||
                        (index < 0) ||
                        (index >= array.Count))
                    {
                        return null;
                    }

                    current = array[index];
                    bracket = segment.IndexOf('[', endBracket + 1);
                }
            }

            return current;
        }

        private static void applyToken(IField field, JsonNode token)
        {
            if (token is JsonValue value && field is IValueField scalar &&
                Importer.tryConvert(value, scalar.ValueType, out object? converted))
            {
                scalar.Value = converted;
                return;
            }

            if (token is JsonArray array && field is IArrayField list)
            {
                List<object> values = new List<object>();
                foreach (JsonNode? item in array)
                {
                    if (item is not JsonValue itemValue ||
                        !Importer.tryConvert(itemValue, list.ValueType, out object? parsed) ||
                        parsed == null)
                    {
                        throw new InvalidOperationException($"Invalid value for field '{field.Key}'.");
                    }

                    values.Add(parsed);
                }

                list.Values = values.ToArray();
                return;
            }

            throw new InvalidOperationException($"Invalid value for field '{field.Key}'.");
        }

        private static bool tryConvert(JsonValue value, Type targetType, out object? result)
        {
            if (!JsonSerialization.TryGetScalar(value, out object? scalar) || (scalar == null))
            {
                result = null;
                return false;
            }

            if (targetType.IsInstanceOfType(scalar))
            {
                result = scalar;
                return true;
            }

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter.CanConvertFrom(scalar.GetType()))
                {
                    result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, scalar);
                    return result != null;
                }

                if (converter.CanConvertFrom(typeof(string)))
                {
                    string? text = Convert.ToString(scalar, CultureInfo.InvariantCulture);
                    if (text == null)
                    {
                        result = null;
                        return false;
                    }

                    result = converter.ConvertFromInvariantString(text);
                    return result != null;
                }
            }
            catch (Exception exception) when (exception is FormatException or NotSupportedException or ArgumentException or OverflowException or InvalidCastException)
            {
                result = null;
                return false;
            }

            result = null;
            return false;
        }

        private async Task<bool> import(Dictionary<FieldKey, JsonNode> tokens, string issueType, Dictionary<FieldKey, FieldInfo> schema)
        {
            IssueKey importKey;
            if (this.tryGetFieldValue(tokens, nameof(IReadOnlyIssue.Key), out string importKeyValue))
            {
                importKey = IssueKey.Create(importKeyValue);
                tokens.Remove(FieldKey.Create(nameof(IReadOnlyIssue.Key)));
            }
            else
            {
                return false;
            }

            string title;
            if (this.tryGetFieldValue(tokens, nameof(IIssue.Title), out string titleValue))
            {
                title = titleValue;
            }
            else
            {
                return false;
            }

            string description;
            if (this.tryGetFieldValue(tokens, nameof(IIssue.Description), out string descriptionValue))
            {
                description = descriptionValue;
            }
            else
            {
                description = string.Empty;
            }

            SyncedIssue synced = await this
                .FirstOrDefaultAsync(s => s.ImportKey == importKey);

            IIssue? issue = null;
            if (synced.IsValid)
            {
                issue = await this.manager
                    .FindAsync(i => i.Key == synced.IssueKey)
                    .FirstOrDefaultAsync();
            }

            if (issue == null)
            {
                List<IField> initial = new List<IField>();
                foreach (KeyValuePair<FieldKey, JsonNode> token in tokens)
                {
                    if (token.Key == "Title" || token.Key == "Description")
                    {
                        continue;
                    }

                    IField field = schema[token.Key].CreateField(null!, token.Key);
                    Importer.applyToken(field, token.Value);
                    initial.Add(field);
                }

                issue = await this.manager.CreateAsync(title, description, issueType, initial);
                synced = new SyncedIssue(this, issue.Key, importKey);
            }
            else if (issue.IssueType != issueType)
            {
                throw new InvalidOperationException($"Issue '{issue.Key}' has type '{issue.IssueType}', not '{issueType}'.");
            }

            if (await this.import(issue, tokens))
            {
                await issue.SaveAsync();
                await synced.SaveAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> import(IIssue issue, Dictionary<FieldKey, JsonNode> tokens)
        {
            foreach (KeyValuePair<FieldKey, JsonNode> token in tokens)
            {
                if (issue.TryGetValue(token.Key, out IField? field))
                {
                    Importer.applyToken(field, token.Value);
                }
            }

            await issue.SaveAsync();
            return true;
        }

        private bool tryGetFieldValue<T>(Dictionary<FieldKey, JsonNode> tokens, string field, out T result)
        {
            if (tokens.TryGetValue(FieldKey.Create(field), out JsonNode? token))
            {
                if (token is JsonValue value && JsonSerialization.TryGetScalar(value, out object? scalar))
                {
                    if (scalar is T output)
                    {
                        result = output;
                        return true;
                    }
                }
            }

            result = default(T)!;
            return false;
        }
    }
}
