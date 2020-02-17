using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Util.Commands.Track;
using Serilog;

namespace GitIssue.Util.Commands.Create
{
    /// <summary>
    ///     Create Command
    /// </summary>
    public class CreateCommand : Command<CreateOptions>
    {
        private Lazy<IIssueFormatter> formatter = new Lazy<IIssueFormatter>(() => new DetailedFormatter());
        private static ILogger? Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(CreateOptions options)
        {
            await using var issues = Initialize(options);
            var issue = await issues.CreateAsync(options.Title, options.Description);
            if (options.Tracked == TrackedIssue.None)
            {
                options.Tracked = new TrackedIssue(issue.Key);
                options.Tracked.Save(Path.Combine(options.Path, options.Name, options.Tracking), Logger);
                Console.WriteLine($"Created and tracking new issue '{issue.Key}'");
            }
            else
            {
                Console.WriteLine($"Created new issue '{issue.Key}'");
            }
        }
    }
}