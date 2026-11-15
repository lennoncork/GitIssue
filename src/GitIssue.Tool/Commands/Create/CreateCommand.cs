using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
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
        private readonly IIssueConfiguration configuration;

        private readonly IEditor editor;

        private readonly ILogger logger;

        private readonly IIssueManager manager;
        private Lazy<IIssueFormatter> formatter = new Lazy<IIssueFormatter>(() => IssueFormatter.Detailed);

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
            string issueType = string.IsNullOrEmpty(options.IssueType) ? this.configuration.IssueType : options.IssueType;
            Dictionary<FieldKey, FieldInfo> schema = this.configuration.GetFields(issueType);
            List<IField> initial = new List<IField>();
            foreach (string assignment in options.Fields)
            {
                int separator = assignment.IndexOf('=');
                if (separator <= 0 ||
                    !schema.TryGetValue(FieldKey.Create(assignment.Substring(0, separator)), out FieldInfo? info) ||
                    assignment.Substring(0, separator) is "IssueType" or "Key")
                {
                    throw new ArgumentException($"Invalid field assignment '{assignment}'.");
                }

                IField field = info.CreateField(null!, FieldKey.Create(assignment.Substring(0, separator)));
                if (!field.Update(assignment.Substring(separator + 1)))
                {
                    throw new ArgumentException($"Invalid field assignment '{assignment}'.");
                }

                initial.Add(field);
            }

            IIssue issue;
            if (string.IsNullOrEmpty(options.Title))
            {
                IField[] fields = schema
                    .Where(f => f.Key != nameof(IReadOnlyIssue.Key))
                    .Where(f => f.Key != nameof(IReadOnlyIssue.IssueType))
                    .Where(f => f.Key != nameof(IIssue.Created))
                    .Where(f => f.Key != nameof(IIssue.Updated))
                    .Select(f => f.Value.CreateField(null!, f.Key))
                    .ToArray();

                await this.editor.Open(fields);

                if (!(fields.FirstOrDefault(f => f.Key == nameof(IIssue.Title)) is IValueField title))
                {
                    this.logger.Error("Title is not a valid value field");
                    this.ExitCode = 1;
                    return;
                }

                if (!(title.Value is IValue<string> value))
                {
                    this.logger.Error($"Title {title} is not a valid string, {title.GetType()}");
                    this.ExitCode = 1;
                    return;
                }

                if (string.IsNullOrEmpty(value.Item))
                {
                    this.logger.Error("A valid title must be provided");
                    this.ExitCode = 1;
                    return;
                }

                foreach (IField field in fields)
                {
                    if (!schema.TryGetValue(field.Key, out FieldInfo? info))
                    {
                        continue;
                    }

                    if (!info.IsValid(field))
                    {
                        continue;
                    }

                    initial.Add(field);
                }

                issue = await this.manager.CreateAsync(value.Item, string.Empty, issueType, initial, options.Location);
            }
            else
            {
                issue = await this.manager.CreateAsync(options.Title, options.Description, issueType, initial, options.Location);
            }

            if (options.Track || (options.Tracked == TrackedIssue.None))
            {
                options.Tracked = new TrackedIssue(issue.Key);
                await options.Tracked.SaveAsync(Path.Combine(options.Path, options.Tracking), this.logger);
                Console.WriteLine($"Created and tracking new issue '{issue.Key}'");
            }
            else
            {
                Console.WriteLine($"Created new issue '{issue.Key}'");
            }
        }
    }
}
