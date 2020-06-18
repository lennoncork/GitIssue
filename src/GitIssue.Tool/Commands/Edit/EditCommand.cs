using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Formatters;
using LibGit2Sharp;
using Serilog;

namespace GitIssue.Util.Commands.Edit
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class EditCommand : Command<EditOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public EditCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        private static bool TryGetEditor(IRepository repository, string key, out string config)
        {
            config = repository.Config.GetValueOrDefault<string>(key);
            return string.IsNullOrEmpty(config) == false;
        }

        /// <inheritdoc />
        public override async Task Exec(EditOptions options)
        {
            var formatter = new DetailedFormatter();
            
            var issue = await manager
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                this.logger.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            if (TryGetEditor(manager.Repository, "issues.editor", out string config))
            {
                options.Editor = config;
            }

            var updated = false;
            if (string.IsNullOrEmpty(options.Field))
            {
                var editor = new Editor
                {
                    Command = options.Editor
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
                this.logger.Error($"Field \"{key}\" does not exist on issue \"{issue.Key}\"");
                return;
            }

            if (string.IsNullOrEmpty(options.Update))
            {
                var editor = new Editor
                {
                    Command = options.Editor
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