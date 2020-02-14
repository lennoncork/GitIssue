using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Util
{
    /// <summary>
    /// Delete command
    /// </summary>
    public class DeleteCommand : Command<DeleteOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options) => Program.Initialize(options);

        /// <inheritdoc />
        public override async Task Exec(DeleteOptions options)
        {
            await using var issues = new IssueManager(options.Path, Logger);
            await issues.DeleteAsync(options.Key);
        }
    }
}