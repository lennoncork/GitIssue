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
        private static ILogger? Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(DeleteOptions options)
        {
            await using var issues = Initialize(options);
            var result = await issues.DeleteAsync(options.Key);
            if (result)
            {
                Console.WriteLine($"Deleted issue '{options.Key}'");
                if (options.Tracked != TrackedIssue.None)
                {
                    if (options.Tracked.Key == options.Key)
                    {
                        options.Tracked = TrackedIssue.None;
                        await options.Tracked.SaveAsync(Path.Combine(options.Path, options.Name, options.Tracking), Logger);
                    }
                }
            }
        }
    }
}