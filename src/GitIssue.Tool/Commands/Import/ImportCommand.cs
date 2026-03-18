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