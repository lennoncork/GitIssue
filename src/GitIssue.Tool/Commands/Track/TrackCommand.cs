using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Issues;
using Serilog;

namespace GitIssue.Tool.Commands.Track
{
    /// <summary>
    ///     Track command
    /// </summary>
    public class TrackCommand : Command<TrackOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public TrackCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(TrackOptions options)
        {
            options.Tracked = TrackedIssue.None;
            if (!string.IsNullOrEmpty(options.Key))
            {
                var find = await manager.FindAsync(i => i.Key.ToString() == options.Key)
                    .FirstOrDefaultAsync();

                if (find != null)
                {
                    options.Tracked.Key = find.Key;
                    options.Tracked.Started = DateTime.Now;
                }
            }

            await options.Tracked.SaveAsync(Path.Combine(options.Path, options.Name, options.Tracking), this.logger);
            Console.WriteLine($"Tacking Issue '{options.Tracked.Key}'");
        }
    }
}