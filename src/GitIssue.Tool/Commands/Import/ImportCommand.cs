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
                    await this.importCsv(options);
                    break;

                case ".json":
                    await this.importJson(options.Import);
                    break;

                default:
                    string message = $"Unsupported import file '{options.Import}'. Use a .json or .csv file.";
                    this.logger.Error(message);
                    throw new InvalidOperationException(message);
            }
        }

        public async Task importJson(string path)
        {
            FileImporter importer = new FileImporter(this.manager)
            {
                ImportPath = path,
            };
            await ImportCommand.applyDefaultMappingAsync(importer);

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
            throw new InvalidOperationException(message);
        }

        private static async Task applyDefaultMappingAsync(FileImporter importer)
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

        public async Task importCsv(ImportOptions options)
        {
            if (!File.Exists(options.Import))
            {
                this.logger.Error($"Import file {options.Import} does not exist");
                return;
            }

            await using FileStream stream = new FileStream(options.Import, FileMode.Open, FileAccess.Read);
            using TextReader reader = new StreamReader(stream);

            string? header = await reader.ReadLineAsync();
            if (header == null)
            {
                return;
            }

            string[] fields = header.Split(options.Separator);
            Dictionary<FieldKey, int> mapping = new Dictionary<FieldKey, int>();
            foreach (KeyValuePair<FieldKey, FieldInfo> field in this.manager.Configuration.Fields)
            {
                int? index = fields.Select((f, i) => new { Field = f, Index = i })
                    .Where(x => x.Field == field.Key.ToString())
                    .Select(x => (int?)x.Index)
                    .FirstOrDefault();

                if (index.HasValue)
                {
                    mapping[field.Key] = index.Value;
                }
            }

            if (!mapping.ContainsKey("Key"))
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
                    continue;
                }

                IIssue? issue = null;
                await foreach (IIssue found in this.manager.FindAsync(i => i.Key == import[mapping["Key"]]))
                {
                    issue = found;
                    Console.WriteLine($"Updating {import[mapping["Key"]]}");
                    break;
                }

                if (issue == null)
                {
                    Console.WriteLine($"Creating {import[mapping["Key"]]}");
                    issue = await this.manager.CreateAsync(import[mapping["Title"]]);
                }

                count++;
            }

            Console.WriteLine();

            Console.WriteLine($"Imported {count} issues from {options.Import}");
        }
    }
}