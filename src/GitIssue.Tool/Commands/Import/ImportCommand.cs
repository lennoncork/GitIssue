using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using Serilog;

namespace GitIssue.Tool.Commands.Import
{
    /// <summary>
    ///     Edit command
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
                default:
                    this.Fail(ImportCommand.UnsupportedImportMessage(options.Import));
                    break;
            }
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

            this.EnsureRequiredColumns(mapping);

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

                string sourceKey = import[mapping["Key"]];
                IIssue? issue = null;
                await foreach (IIssue found in this.manager.FindAsync(i => i.Key == sourceKey))
                {
                    issue = found;
                    break;
                }

                if (issue != null)
                {
                    if (!options.Overwrite)
                    {
                        Console.WriteLine($"Skipping {issue.Key}; it already exists. Use --Update to overwrite.");
                        continue;
                    }

                    Console.WriteLine($"Updating {issue.Key}");
                    this.ApplyFields(issue, mapping, import);
                    issue.Updated = DateTime.Now;
                    await issue.SaveAsync();
                }
                else
                {
                    Console.WriteLine($"Creating {sourceKey}");
                    issue = await this.manager.CreateAsync(import[mapping["Title"]]);
                    this.ApplyFields(issue, mapping, import);
                    issue.Updated = DateTime.Now;
                    await issue.SaveAsync();
                }

                count++;
            }

            Console.WriteLine();

            Console.WriteLine($"Imported {count} issues from {options.Import}");
        }

        private void ApplyFields(IIssue issue, IReadOnlyDictionary<FieldKey, int> mapping, string[] values)
        {
            foreach (KeyValuePair<FieldKey, int> map in mapping)
            {
                if (map.Key == "Key")
                {
                    continue;
                }

                if (!issue.ContainsKey(map.Key))
                {
                    this.logger.Error($"Field \"{map.Key}\" does not exist on issue \"{issue.Key}\"");
                    continue;
                }

                if (!issue[map.Key].Update(values[map.Value]))
                {
                    this.logger.Error($"Failed to update field \"{map.Key}\" on issue \"{issue.Key}\"");
                }
            }
        }

        private void EnsureRequiredColumns(IReadOnlyDictionary<FieldKey, int> mapping)
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
                return;
            }

            string columnList = missing.Count == 1 ? missing[0] : string.Join(" and ", missing);
            string suffix = missing.Count == 1 ? "column" : "columns";
            this.Fail($"CSV import requires the {columnList} {suffix}.");
        }

        private void Fail(string message)
        {
            this.logger.Error(message);
            throw new InvalidOperationException(message);
        }

        private static string UnsupportedImportMessage(string path)
        {
            string extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                return $"Import file \"{path}\" is not supported. Only .csv files can be imported.";
            }

            return $"Import of \"{extension}\" files is not supported. Only .csv files can be imported.";
        }
    }
}
