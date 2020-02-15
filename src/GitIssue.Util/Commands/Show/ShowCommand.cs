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
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(ShowOptions options)
        {
            var formatter = new DetailedFormatter();
            await using var issues = Initialize(options);
            var issue = await issues
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();
            Console.WriteLine(issue?.Format(formatter));
        }
    }
}