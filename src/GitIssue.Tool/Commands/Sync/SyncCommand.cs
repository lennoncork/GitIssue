using System;
using System.Threading.Tasks;
using GitIssue.Syncs;
using Serilog;

namespace GitIssue.Util.Commands.Sync
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class SyncCommand : Command<SyncOptions>
    {
        private static ILogger? Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(SyncOptions options)
        {
            await using var issues = Initialize(options);

            var importer = new FileImporter(issues);

            Console.WriteLine($"Exported issues to {options.Import}");
        }
    }
}