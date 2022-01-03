using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields.Value;
using GitIssue.Formatters;
using GitIssue.Issues;
using GitIssue.Values;
using Serilog;

namespace GitIssue.Tool.Commands.Create
{
    /// <summary>
    ///     Create Command
    /// </summary>
    public class CreateCommand : Command<CreateOptions>
    {
        private Lazy<IIssueFormatter> formatter = new Lazy<IIssueFormatter>(() => IssueFormatter.Detailed);

        private readonly ILogger logger;

        private readonly IEditor editor;

        private readonly IIssueManager manager;

        private readonly IIssueConfiguration configuration;

        public CreateCommand(IIssueManager manager, IIssueConfiguration configuration, IEditor editor, ILogger logger)
        {
            this.manager = manager;
            this.configuration = configuration;
            this.editor = editor;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(CreateOptions options)
        {
            IIssue issue;
            if (string.IsNullOrEmpty(options.Title))
            {
                var fields = this.configuration.Fields
                    .Where(f => f.Key != nameof(IIssue.Key))
                    .Where(f => f.Key != nameof(IIssue.Created))
                    .Where(f => f.Key != nameof(IIssue.Updated))
                    .Select(f => f.Value.CreateField(null!, f.Key))
                    .ToArray();

                await editor.Open(fields);

                if (!(fields.FirstOrDefault(f => f.Key == nameof(IIssue.Title)) is IValueField title))
                {
                    this.logger.Error($"Title is not a valid value field");
                    return;
                }

                if (!(title.Value is IValue<string> value))
                {
                    this.logger.Error($"Title {title} is not a valid string, {title.GetType()}");
                    return;
                }

                if (string.IsNullOrEmpty(value.Item))
                {
                    this.logger.Error($"A valid title must be provided");
                }

                issue = await manager.CreateAsync(value.Item);
                foreach (var field in fields)
                {
                    issue.SetField(field.Key).WithField(field);
                }
            }
            else
            {
                issue = await manager.CreateAsync(options.Title, options.Description);
            }

            if (options.Track || options.Tracked == TrackedIssue.None)
            {
                options.Tracked = new TrackedIssue(issue.Key);
                await options.Tracked.SaveAsync(Path.Combine(options.Path, options.Name, options.Tracking), this.logger);
                Console.WriteLine($"Created and tracking new issue '{issue.Key}'");
            }
            else
            {
                Console.WriteLine($"Created new issue '{issue.Key}'");
            }
        }
    }
}