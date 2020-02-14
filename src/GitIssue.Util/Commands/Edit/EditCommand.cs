using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Formatters;
using Serilog;

namespace GitIssue.Util
{
    /// <summary>
    /// Edit command
    /// </summary>
    public class EditCommand :  Command<EditOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options) => Program.Initialize(options);

        /// <inheritdoc />
        public override async Task Exec(EditOptions options)
        {
            var formatter = new DetailedFormatter();
            await using var issues = Initialize(options);
            var issue = await issues
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                Logger.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            var updated = false;
            if (string.IsNullOrEmpty(options.Field))
            {
                var editor = new Editor
                {
                    Command = options.Editor,
                    Arguments = options.Arguments
                };
                await editor.Open(issue);
                updated = true;
            }

            if (updated)
            {
                issue.Updated = DateTime.Now;
                if (await issue.SaveAsync()) Console.WriteLine(issue.Format(formatter));
                return;
            }

            var key = FieldKey.Create(options.Field);
            if (!issue.ContainsKey(key))
            {
                Logger.Error($"Field \"{key}\" does not exist on issue \"{issue.Key}\"");
                return;
            }

            if (string.IsNullOrEmpty(options.Update))
            {
                var editor = new Editor
                {
                    Command = options.Editor,
                    Arguments = options.Arguments
                };
                await editor.Open(issue[key]);
                updated = true;
            }
            else if (await issue[key].UpdateAsync(options.Update))
            {
                updated = true;
            }

            if (updated)
            {
                issue.Updated = DateTime.Now;
                if (await issue.SaveAsync()) Console.WriteLine(issue.Format(formatter));
            }
        }
    }
}