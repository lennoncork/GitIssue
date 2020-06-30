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
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public SyncCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override Task Exec(SyncOptions options)
        {
            var importer = new FileImporter(manager);
            Console.WriteLine($"Exported issues to {options.Import}");
            return Task.CompletedTask;
        }
    }
}