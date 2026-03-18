using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Issues;
using Serilog;

namespace GitIssue.Tool.Commands.Remove
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class RemoveCommand : Command<RemoveOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public RemoveCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(RemoveOptions options)
        {
            TerminalFormatter formatter = TerminalFormatter.Detailed;

            IIssue? issue = await this.manager
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                this.logger.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            if (string.IsNullOrEmpty(options.Field))
            {
                this.logger.Error($"Field must be supplied to remove a value on \"{issue.Key}\"");
                return;
            }

            FieldKey key = FieldKey.Create(options.Field);
            if (!issue.ContainsKey(key))
            {
                this.logger.Error($"Field \"{key}\" does not exist on issue \"{issue.Key}\"");
                return;
            }

            IField field = issue[key];
            if (field is IArrayField arrayField)
            {
                if (arrayField.TryParse(options.Remove, out object? value))
                {
                    if (arrayField.Contains(value))
                    {
                        arrayField.Remove(value);
                        await issue.SaveAsync();
                        Console.WriteLine(formatter.Format(arrayField));
                    }
                    else
                    {
                        this.logger.Error($"Item \"{options.Remove}\" does not exist on field {options.Field}");
                    }
                }
                else
                {
                    this.logger.Error($"Item \"{options.Remove}\" is not a valid value for field {options.Field}");
                }
            }
            else
            {
                this.logger.Error($"Field \"{key}\" is not an array type, cannot remove value");
            }
        }
    }
}