using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Formatters;
using Serilog;

namespace GitIssue.Util.Commands.Show
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
            var formatter = new DetailedFormatter();
            var issue = await manager
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();
            Console.WriteLine(issue?.Format(formatter));
        }
    }
}