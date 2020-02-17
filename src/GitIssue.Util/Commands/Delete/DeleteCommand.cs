using System;
using System.Threading.Tasks;
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
            if(result)
                Console.WriteLine($"Deleted issue '{options.Key}'");
        }
    }
}