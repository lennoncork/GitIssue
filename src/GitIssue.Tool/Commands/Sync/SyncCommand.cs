using System.Threading.Tasks;
using GitIssue.Tool.Commands.Import;
using Serilog;

namespace GitIssue.Tool.Commands.Sync
{
    /// <summary>
    ///     Sync command
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
            ImportCommand import = new ImportCommand(this.manager, this.logger);
            return import.Exec(new ImportOptions
            {
                Import = options.Import,
            });
        }
    }
}
