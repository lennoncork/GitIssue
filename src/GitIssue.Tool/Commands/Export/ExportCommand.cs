using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace GitIssue.Util.Commands.Export
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
            var formatter = new DetailedFormatter();

            JObject json = new JObject();
            await foreach (var issue in manager.FindAsync(i => true))
            {
                if (issue is IJsonIssue jsonIssue)
                {
                    json[issue.Key] = jsonIssue.ToJson();
                }
            }

            if (File.Exists(options.Export) && options.Overwrite == false)
            {
                this.logger.Error($"Export file {options.Export} exists, use '{nameof(ExportOptions.Overwrite)}' to force");
                return;
            }

            await using var stream = new FileStream(options.Export, FileMode.Create, FileAccess.ReadWrite);
            using JsonWriter writer = new JsonTextWriter(new StreamWriter(stream));
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, json);

            Console.WriteLine($"Exported issues to {options.Export}");
        }
    }
}