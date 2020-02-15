using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Util.Commands.Init
{
    /// <summary>
    ///     Init command
    /// </summary>
    public class InitCommand : Command<InitOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(InitOptions options)
        {
            await using var issues = IssueManager.Init(options.Path, options.Name);
        }
    }
}