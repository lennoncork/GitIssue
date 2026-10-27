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
            Dictionary<FieldKey, JsonNode> tokens = new Dictionary<FieldKey, JsonNode>();
            foreach (KeyValuePair<string, string> kvp in this.Configuration.Mapping)
            {
                FieldKey key = FieldKey.Create(kvp.Key);
                if (this.manager.Configuration.Fields.ContainsKey(key))
                {
                    JsonNode? token = Importer.SelectNode(json, kvp.Value);
                    if (token != null)
                    {
                        tokens[key] = token;
                    }
                }
            }

            return await this.Import(tokens);
        }

        private static JsonNode? SelectNode(JsonNode node, string path)
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

        private static bool TryConvert(JsonValue value, Type targetType, out object? result)
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
            catch (Exception)
            {
                // Conversion failures leave the existing field unchanged.
            }

            result = null;
            return false;
        }

        private async Task<bool> Import(Dictionary<FieldKey, JsonNode> tokens)
        {
            IssueKey importKey;
            if (this.TryGetFieldValue(tokens, nameof(IReadOnlyIssue.Key), out string importKeyValue))
            {
                importKey = IssueKey.Create(importKeyValue);
                tokens.Remove(FieldKey.Create(nameof(IReadOnlyIssue.Key)));
            }
            else
            {
                return false;
            }

            string title;
            if (this.TryGetFieldValue(tokens, nameof(IIssue.Title), out string titleValue))
            {
                title = titleValue;
            }
            else
            {
                return false;
            }

            string description;
            if (this.TryGetFieldValue(tokens, nameof(IIssue.Description), out string descriptionValue))
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
                issue = await this.manager.CreateAsync(title, description);
                synced = new SyncedIssue(this, issue.Key, importKey);
            }

            if (await this.Import(issue, tokens))
            {
                await issue.SaveAsync();
                await synced.SaveAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> Import(IIssue issue, Dictionary<FieldKey, JsonNode> tokens)
        {
            foreach (KeyValuePair<FieldKey, JsonNode> token in tokens)
            {
                if (issue.TryGetValue(token.Key, out IField? field))
                {
                    if (token.Value is JsonValue value)
                    {
                        if (field is IValueField valueField)
                        {
                            if (Importer.TryConvert(value, valueField.ValueType, out object? converted))
                            {
                                valueField.Value = converted;
                            }
                        }
                    }

                    if (token.Value is JsonArray array)
                    {
                        if (field is IArrayField arrayField)
                        {
                            arrayField.Clear();
                            foreach (JsonNode? item in array)
                            {
                                if (item is JsonValue itemValue &&
                                    Importer.TryConvert(itemValue, arrayField.ValueType, out object? converted))
                                {
                                    arrayField.Add(converted);
                                }
                            }
                        }
                    }
                }
            }

            await issue.SaveAsync();
            return true;
        }

        private bool TryGetFieldValue<T>(Dictionary<FieldKey, JsonNode> tokens, string field, out T result)
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