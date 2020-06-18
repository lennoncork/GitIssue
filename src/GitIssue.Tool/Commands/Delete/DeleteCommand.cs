using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using Serilog;

namespace GitIssue.Util.Commands.Delete
{
    /// <summary>
    ///     Delete command
    /// </summary>
    public class DeleteCommand : Command<DeleteOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public DeleteCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(DeleteOptions options)
        {
            var result = await manager.DeleteAsync(options.Key);
            if (result)
            {
                Console.WriteLine($"Deleted issue '{options.Key}'");
                if (options.Tracked != TrackedIssue.None)
                {
                    if (options.Tracked.Key == options.Key)
                    {
                        options.Tracked = TrackedIssue.None;
                        await options.Tracked.SaveAsync(Path.Combine(options.Path, options.Name, options.Tracking), this.logger);
                    }
                }
            }
        }
    }
}