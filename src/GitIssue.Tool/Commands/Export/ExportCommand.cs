using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace GitIssue.Tool.Commands.Export
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class ExportCommand : Command<ExportOptions>
    {
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
                    await this.exportJson(options);
                    break;

                case ".csv":
                    await this.exportCsv(options);
                    break;
            }
        }

        public async Task exportCsv(ExportOptions options)
        {
            if (File.Exists(options.Export) && !options.Overwrite)
            {
                this.logger.Error($"Export file {options.Export} exists, use '{nameof(ExportOptions.Overwrite)}' to force");
                return;
            }

            await using FileStream stream = new FileStream(options.Export, FileMode.Create, FileAccess.ReadWrite);
            using TextWriter writer = new StreamWriter(stream);

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<FieldKey, FieldInfo> field in this.manager.Configuration.Fields)
            {
                if (builder.Length != 0)
                {
                    builder.Append(options.Separator);
                }

                builder.Append(field.Key.ToString());
            }

            await writer.WriteLineAsync(builder.ToString());

            int count = 0;
            await foreach (IIssue issue in this.manager.FindAsync(i => true))
            {
                builder.Clear();
                foreach (KeyValuePair<FieldKey, FieldInfo> field in this.manager.Configuration.Fields)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(options.Separator);
                    }

                    builder.Append(issue[field.Key]);
                }

                await writer.WriteLineAsync(builder.ToString());
                count++;
            }

            Console.WriteLine($"Exported {count} issues to {options.Export}");
        }

        public async Task exportJson(ExportOptions options)
        {
            if (File.Exists(options.Export) && !options.Overwrite)
            {
                this.logger.Error($"Export file {options.Export} exists, use '{nameof(ExportOptions.Overwrite)}' to force");
                return;
            }

            JObject json = new JObject();
            await foreach (IIssue issue in this.manager.FindAsync(i => true))
            {
                if (issue is IJsonIssue jsonIssue)
                {
                    json[issue.Key] = jsonIssue.ToJson();
                }
            }

            await using FileStream stream = new FileStream(options.Export, FileMode.Create, FileAccess.ReadWrite);
            using JsonWriter writer = new JsonTextWriter(new StreamWriter(stream));
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, json);

            Console.WriteLine($"Exported {json.Count} issues to {options.Export}");
        }
    }
}