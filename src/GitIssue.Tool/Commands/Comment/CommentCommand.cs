using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Formatters;
using Serilog;

using String = GitIssue.Values.String;

namespace GitIssue.Util.Commands.Add
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class CommentCommand : Command<CommentOptions>
    {
        private readonly static string CommentField = "Comments";

        private readonly ILogger logger;
        
        private readonly IIssueManager manager;

        public CommentCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(CommentOptions options)
        {
            var formatter = new DetailedFormatter();

            var issue = await manager
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                this.logger?.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            if (string.IsNullOrEmpty(options.Comment))
            {
                var editor = new Editor
                {
                    Command = options.Editor
                };

                options.Comment = await editor.Edit($"Add a comment to {issue.Key} below", "");
            }

            if (string.IsNullOrEmpty(options.Comment))
            {
                this.logger?.Error($"Comment \"{options.Comment}\" is not valid");
                return;
            }

            var key = FieldKey.Create(CommentField);
            if(issue.TryGetValue(key, out var field))
            {
                if(field is IArrayField arrayField)
                {
                    if (arrayField.TryParse(options.Comment, out var value))
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