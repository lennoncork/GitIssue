using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Issues;
using Serilog;
using String = GitIssue.Values.String;

namespace GitIssue.Tool.Commands.Comment
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class CommentCommand : Command<CommentOptions>
    {
        private static readonly string CommentField = "Comments";

        private readonly IEditor editor;

        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public CommentCommand(IIssueManager manager, IEditor editor, ILogger logger)
        {
            this.manager = manager;
            this.editor = editor;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(CommentOptions options)
        {
            TerminalFormatter formatter = TerminalFormatter.Detailed;

            IIssue? issue = await this.manager
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                this.logger?.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            if (string.IsNullOrEmpty(options.Comment))
            {
                options.Comment = await this.editor.Edit($"Add a comment to {issue.Key} below", "");
            }

            if (string.IsNullOrEmpty(options.Comment))
            {
                this.logger?.Error($"Comment \"{options.Comment}\" is not valid");
                return;
            }

            FieldKey key = FieldKey.Create(CommentCommand.CommentField);
            if (issue.TryGetValue(key, out IField? field))
            {
                if (field is IArrayField arrayField)
                {
                    if (arrayField.TryParse(options.Comment, out object? value))
                    {
                        arrayField.Add(value);
                        await issue.SaveAsync();
                        Console.WriteLine(formatter.Format(arrayField));
                    }
                    else
                    {
                        this.logger?.Error($"Comment \"{options.Comment}\" is not valid");
                    }

                    arrayField.Add(String.Parse(options.Comment));
                }
            }
        }
    }
}