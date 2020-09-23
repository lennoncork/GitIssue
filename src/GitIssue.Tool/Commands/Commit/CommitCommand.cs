using System;
using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Tool.Commands.Commit
{
    /// <summary>
    ///     Commit command
    /// </summary>
    public class CommitCommand : Command<CommitOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public CommitCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(CommitOptions options)
        {
            var result = await manager.CommitAsync();
            if (result)
            {
                Console.WriteLine($"Committed changes in {manager.Root.Name}, see log for details");
            }
        }
    }
}