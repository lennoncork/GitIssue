using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using Serilog;

namespace GitIssue.Tool.Commands.Show
{
    /// <summary>
    ///     Show command
    /// </summary>
    public class ShowCommand : Command<ShowOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public ShowCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(ShowOptions options)
        {
            var formatter = new TerminalFormatter("%*");

            IAsyncEnumerable<IIssue> issues;
            switch(options.Show)
            {
                case ShowSubCommand.tracked:
                case ShowSubCommand.Tracked:
                    issues = manager.FindAsync(i => i.Key.ToString() == options.Key);
                    break;

                case ShowSubCommand.all:
                case ShowSubCommand.All:
                    issues = manager.FindAsync(i => true);
                    break;

                case ShowSubCommand.mine:
                case ShowSubCommand.Mine:
                    issues = manager.FindAsync(i => true);
                    break;

                default:
                    issues = AsyncEnumerable.Empty<IIssue>();
                    break;
            }

            int count = 0;
            await foreach(var issue in issues)
            {
                if (count++ > 0) Console.WriteLine();
                Console.WriteLine(issue.Format(formatter));
            }    
        }
    }
}