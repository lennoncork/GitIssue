using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Util
{
    /// <summary>
    /// Create Command
    /// </summary>
    public class CreateCommand: Command<CreateOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options) => Program.Initialize(options);

        /// <inheritdoc />
        public override async Task Exec(CreateOptions options)
        {
            await using var issues = Initialize(options);
            await issues.CreateAsync(options.Title, options.Description);
        }
    }
}