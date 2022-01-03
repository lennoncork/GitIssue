using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Issues;
using Newtonsoft.Json.Linq;

namespace GitIssue.Syncs
{
    /// <summary>
    /// Imports issues from a JSON description
    /// </summary>
    public abstract class Importer : IImporter
    {
        private readonly IIssueManager manager;

        /// <summary>
        /// Initializes an instance of the <see cref="Importer"/> class
        /// </summary>
        /// <param name="manager"></param>
        public Importer(IIssueManager manager)
        {
            this.Root = new SyncRoot(manager.Root, "imports");
            this.manager = manager;
            this.Configuration = new SyncConfiguration();
        }

        /// <summary>
        /// Gets the base path for imported issues
        /// </summary>
        public SyncRoot Root { get; set; }

        /// <summary>
        /// The configuration of the sync
        /// </summary>
        public SyncConfiguration Configuration { get; set; }

        /// <inheritdoc />
        public abstract Task<bool> Import();

        /// <inheritdoc />
        public async IAsyncEnumerator<SyncedIssue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (Directory.Exists(this.Root.ImportPath) == false)
                yield break;

            var files = Directory
                .EnumerateFiles(this.Root.ImportPath, "*.import", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return await SyncedIssue.ReadAsync(this, file);
            }
        }

        /// <summary>
        /// Imports an issue from a JSON object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        protected async Task<bool> Import(JObject json)
        {
            var tokens = new Dictionary<FieldKey, JToken>();
            foreach (var kvp in this.Configuration.Mapping)
            {
                FieldKey key = FieldKey.Create(kvp.Key);
                if (manager.Configuration.Fields.ContainsKey(key))
                {
                    JToken? token = json.SelectToken(kvp.Value);
                    if (token != null) tokens[key] = token;
                }
            }

            return await Import(tokens);
        }

        private async Task<bool> Import(Dictionary<FieldKey, JToken> tokens)
        {
            IssueKey importKey;
            if (TryGetFieldValue<string>(tokens, nameof(IIssue.Key), out var importKeyValue))
            {
                importKey = IssueKey.Create(importKeyValue);
                tokens.Remove(FieldKey.Create(nameof(IIssue.Key)));
            }
            else
            {
                return false;
            }

            string title;
            if (TryGetFieldValue<string>(tokens, nameof(IIssue.Title), out var titleValue))
            {
                title = titleValue;
            }
            else
            {
                return false;
            }

            string description;
            if (TryGetFieldValue<string>(tokens, nameof(IIssue.Description), out var descriptionValue))
            {
                description = descriptionValue;
            }
            else
            {
                description = string.Empty;
            }

            var synced = await this
                .FirstOrDefaultAsync(s => s.ImportKey == importKey);

            IIssue? issue = null;
            if (synced.IsValid)
            {
                issue = await manager
                    .FindAsync(i => i.Key == importKey)
                    .FirstOrDefaultAsync();
            }

            if (issue == null)
            {
                issue = await manager.CreateAsync(title, description);
                synced = new SyncedIssue(this, issue.Key, importKey);
            }

            if (await Import(issue, tokens))
            {
                await issue.SaveAsync();
                await synced.SaveAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> Import(IIssue issue, Dictionary<FieldKey, JToken> tokens)
        {
            foreach (var token in tokens)
            {
                if (issue.TryGetValue(token.Key, out var field))
                {
                    if (token.Value is JValue value)
                    {
                        if (field is IValueField valueField)
                        {
                            var converted = value.ToObject(valueField.ValueType);
                            valueField.Value = converted;
                        }
                    }

                    if (token.Value is JArray array)
                    {
                        if (field is IArrayField arrayField)
                        {
                            arrayField.Clear();
                            foreach (var item in array)
                            {
                                var converted = item.ToObject(arrayField.ValueType);
                                arrayField.Add(converted);
                            }
                        }
                    }
                }
            }

            await issue.SaveAsync();
            return true;
        }

        private bool TryGetFieldValue<T>(Dictionary<FieldKey, JToken> tokens, string field, out T result)
        {
            if (tokens.TryGetValue(FieldKey.Create(field), out var token))
            {
                if (token is JValue value)
                {
                    if (value.Value is T output)
                    {
                        result = output;
                        return true;
                    }
                }
            }
            result = default!;
            return false;
        }
    }
}
