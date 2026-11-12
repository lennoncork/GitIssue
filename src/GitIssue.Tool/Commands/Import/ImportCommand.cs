using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Syncs;
using Serilog;

namespace GitIssue.Tool.Commands.Import
{
    /// <summary>
    ///     Import command
    /// </summary>
    public class ImportCommand : Command<ImportOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public ImportCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(ImportOptions options)
        {
            switch (Path.GetExtension(options.Import))
            {
                case ".csv":
                    await this.ImportCsv(options);
                    break;

                case ".json":
                    await this.ImportJson(options.Import);
                    break;

                default:
                    this.logger.Error($"Unsupported import file '{options.Import}'. Use a .json or .csv file.");
                    this.ExitCode = 1;
                    break;
            }
        }

        public async Task ImportJson(string path)
        {
            FileImporter importer = new FileImporter(this.manager)
            {
                ImportPath = path,
            };
            await ImportCommand.applyDefaultMappingAsync(importer, this.manager.Configuration);

            bool imported = await importer.Import();
            int count = imported ? 1 : 0;
            Console.WriteLine($"Imported {count} issues from {path}");
            if (imported)
            {
                return;
            }

            string message = File.Exists(path)
                ? $"Failed to import issues from {path}. The JSON object must include Key and Title."
                : $"Import file {path} does not exist";
            this.logger.Error(message);
            this.ExitCode = 1;
        }

        private static async Task applyDefaultMappingAsync(FileImporter importer, IIssueConfiguration configuration)
        {
            string key = "Key";
            string title = "Title";
            string description = "Description";
            if (File.Exists(importer.ImportPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(importer.ImportPath);
                    if (JsonNode.Parse(content) is JsonObject json)
                    {
                        key = ImportCommand.mapProperty(json, "Key", "key");
                        title = ImportCommand.mapProperty(json, "Title", "title");
                        description = ImportCommand.mapProperty(json, "Description", "description");
                        IEnumerable<Dictionary<FieldKey, FieldInfo>> schemas = configuration.Issues.Count == 0
                            ? new[] { configuration.GetFields(configuration.IssueType) }
                            : configuration.Issues.Select(issue => configuration.GetFields(issue.Name));
                        foreach (FieldKey field in schemas.SelectMany(schema => schema.Keys).Distinct())
                        {
                            if (json.ContainsKey(field.ToString()))
                            {
                                importer.Configuration.Mapping[field.ToString()] = field.ToString();
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // FileImporter reports JSON that it cannot read.
                }
            }

            importer.Configuration.Mapping["Key"] = key;
            importer.Configuration.Mapping["Title"] = title;
            importer.Configuration.Mapping["Description"] = description;
        }

        private static string mapProperty(JsonObject json, string name, string alias)
        {
            if (json.ContainsKey(name) || !json.ContainsKey(alias))
            {
                return name;
            }

            return alias;
        }

        public async Task ImportCsv(ImportOptions options)
        {
            if (!File.Exists(options.Import))
            {
                this.logger.Error($"Import file {options.Import} does not exist");
                this.ExitCode = 1;
                return;
            }

            await using FileStream stream = new FileStream(options.Import, FileMode.Open, FileAccess.Read);
            using TextReader reader = new StreamReader(stream);

            string? header = await reader.ReadLineAsync();
            if (header == null)
            {
                this.logger.Error($"Import file {options.Import} is empty");
                this.ExitCode = 1;
                return;
            }

            string[] fields = header.Split(options.Separator);
            Dictionary<FieldKey, int> mapping = new Dictionary<FieldKey, int>();
            IEnumerable<Dictionary<FieldKey, FieldInfo>> schemas = this.manager.Configuration.Issues.Count == 0
                ? new[] { this.manager.Configuration.GetFields(this.manager.Configuration.IssueType) }
                : this.manager.Configuration.Issues.Select(issue => this.manager.Configuration.GetFields(issue.Name));
            foreach (FieldKey field in schemas.SelectMany(schema => schema.Keys).Distinct())
            {
                int? index = fields.Select((f, i) => new { Field = f, Index = i })
                    .Where(x => x.Field == field.ToString())
                    .Select(x => (int?)x.Index)
                    .FirstOrDefault();

                if (index.HasValue)
                {
                    mapping[field] = index.Value;
                }
            }

            if (!this.ensureRequiredColumns(mapping))
            {
                return;
            }

            foreach (KeyValuePair<FieldKey, int> map in mapping)
            {
                Console.WriteLine($"[{map.Value}] => {map.Key}");
            }

            Console.WriteLine();

            int count = 0;
            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                string[] import = line.Split(options.Separator);
                if (import.Length != fields.Length)
                {
                    throw new InvalidDataException("A CSV row does not match the header column count.");
                }

                string sourceKey = import[mapping["Key"]];
                string issueType = mapping.TryGetValue("IssueType", out int typeIndex)
                    ? import[typeIndex]
                    : this.manager.Configuration.IssueType;
                Dictionary<FieldKey, FieldInfo> schema = this.manager.Configuration.GetFields(issueType);
                IIssue? issue = null;
                await foreach (IIssue found in this.manager.FindAsync(i => i.Key == sourceKey))
                {
                    issue = found;
                    break;
                }

                if (issue != null)
                {
                    if (issue.IssueType != issueType)
                    {
                        throw new InvalidOperationException($"Issue '{issue.Key}' has type '{issue.IssueType}', not '{issueType}'.");
                    }

                    if (!options.Overwrite)
                    {
                        Console.WriteLine($"Skipping {issue.Key}; it already exists. Use --Update to overwrite.");
                        continue;
                    }

                    Console.WriteLine($"Updating {issue.Key}");
                    this.applyFields(issue, mapping, import);
                    issue.Updated = DateTime.Now;
                    await issue.SaveAsync();
                }
                else
                {
                    Console.WriteLine($"Creating {sourceKey}");
                    List<IField> initial = new List<IField>();
                    foreach (KeyValuePair<FieldKey, int> map in mapping)
                    {
                        if (map.Key == "Key" || map.Key == "IssueType" || map.Key == "Title" ||
                            string.IsNullOrEmpty(import[map.Value]))
                        {
                            continue;
                        }

                        if (!schema.TryGetValue(map.Key, out FieldInfo? info))
                        {
                            throw new InvalidOperationException($"Field '{map.Key}' does not exist on issue type '{issueType}'.");
                        }

                        IField field = info.CreateField(null!, map.Key);
                        if (!field.Update(import[map.Value]))
                        {
                            throw new InvalidOperationException($"Invalid value for field '{map.Key}'.");
                        }

                        initial.Add(field);
                    }

                    issue = await this.manager.CreateAsync(import[mapping["Title"]], string.Empty, issueType, initial);
                }

                count++;
            }

            Console.WriteLine();

            Console.WriteLine($"Imported {count} issues from {options.Import}");
        }

        private void applyFields(IIssue issue, IReadOnlyDictionary<FieldKey, int> mapping, string[] values)
        {
            foreach (KeyValuePair<FieldKey, int> map in mapping)
            {
                if (map.Key == "Key" || map.Key == "IssueType")
                {
                    continue;
                }

                if (!issue.ContainsKey(map.Key))
                {
                    if (!string.IsNullOrEmpty(values[map.Value]))
                    {
                        throw new InvalidOperationException($"Field \"{map.Key}\" does not exist on issue \"{issue.Key}\"");
                    }

                    continue;
                }

                if (!issue[map.Key].Update(values[map.Value]))
                {
                    throw new InvalidOperationException($"Failed to update field \"{map.Key}\" on issue \"{issue.Key}\"");
                }
            }
        }

        private bool ensureRequiredColumns(IReadOnlyDictionary<FieldKey, int> mapping)
        {
            List<string> missing = new List<string>();
            if (!mapping.ContainsKey("Key"))
            {
                missing.Add("Key");
            }

            if (!mapping.ContainsKey("Title"))
            {
                missing.Add("Title");
            }

            if (missing.Count == 0)
            {
                return true;
            }

            string columnList = missing.Count == 1 ? missing[0] : string.Join(" and ", missing);
            string suffix = missing.Count == 1 ? "column" : "columns";
            this.logger.Error($"CSV import requires the {columnList} {suffix}.");
            this.ExitCode = 1;
            return false;
        }
    }
}
