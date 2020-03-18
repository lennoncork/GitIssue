using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using GitIssue.Util.Commands.Track;
using Serilog;

namespace GitIssue.Util.Commands.Create
{
    /// <summary>
    ///     Create Command
    /// </summary>
    public class ChangesCommand : Command<ChangesOptions>
    {
        private Lazy<IIssueFormatter> formatter = new Lazy<IIssueFormatter>(() => new DetailedFormatter());
        private static ILogger? Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(ChangesOptions options)
        {
            await using var issues = Initialize(options);
            Console.Write(issues.Changes.GenerateComments());
        }
    }
}