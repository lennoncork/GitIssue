using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using Serilog;

namespace GitIssue.Tool.Commands.Export
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class ExportCommand : Command<ExportOptions>
    {
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public ExportCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(ExportOptions options)
        {
            switch (Path.GetExtension(options.Export))
            {
                case ".json":
                    await this.ExportJson(options);
                    break;

                case ".csv":
                    await this.ExportCsv(options);
                    break;

                default:
                    this.logger.Error($"Export of '{Path.GetExtension(options.Export)}' files is not supported");
                    this.ExitCode = 1;
                    break;
            }
        }

        public async Task ExportCsv(ExportOptions options)
        {
            if (File.Exists(options.Export) && !options.Overwrite)
            {
                this.logger.Error($"Export file {options.Export} exists, use '{nameof(ExportOptions.Overwrite)}' to force");
                this.ExitCode = 1;
                return;
            }

            await using FileStream stream = new FileStream(options.Export, FileMode.Create, FileAccess.ReadWrite);
            using TextWriter writer = new StreamWriter(stream);

            IEnumerable<Dictionary<FieldKey, FieldInfo>> schemas = this.manager.Configuration.Issues.Count == 0
                ? new[] { this.manager.Configuration.GetFields(this.manager.Configuration.IssueType) }
                : this.manager.Configuration.Issues.Select(issue => this.manager.Configuration.GetFields(issue.Name));
            FieldKey[] columns = schemas.SelectMany(schema => schema.Keys).Distinct().ToArray();
            StringBuilder builder = new StringBuilder();
            foreach (FieldKey field in columns)
            {
                if (builder.Length != 0)
                {
                    builder.Append(options.Separator);
                }

                builder.Append(field.ToString());
            }

            await writer.WriteLineAsync(builder.ToString());

            int count = 0;
            await foreach (IIssue issue in this.manager.FindAsync(i => true))
            {
                builder.Clear();
                foreach (FieldKey field in columns)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(options.Separator);
                    }

                    if (issue.TryGetValue(field, out IField? value))
                    {
                        builder.Append(value);
                    }
                }

                await writer.WriteLineAsync(builder.ToString());
                count++;
            }

            Console.WriteLine($"Exported {count} issues to {options.Export}");
        }

        public async Task ExportJson(ExportOptions options)
        {
            if (File.Exists(options.Export) && !options.Overwrite)
            {
                this.logger.Error($"Export file {options.Export} exists, use '{nameof(ExportOptions.Overwrite)}' to force");
                this.ExitCode = 1;
                return;
            }

            JsonObject json = new JsonObject();
            await foreach (IIssue issue in this.manager.FindAsync(i => true))
            {
                if (issue is IJsonIssue jsonIssue)
                {
                    json[issue.Key] = jsonIssue.ToJson();
                }
            }

            await File.WriteAllTextAsync(options.Export, json.ToJsonString(ExportCommand.jsonOptions));

            Console.WriteLine($"Exported {json.Count} issues to {options.Export}");
        }
    }
}
