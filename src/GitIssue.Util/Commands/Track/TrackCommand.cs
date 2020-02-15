using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Util.Commands.Track
{
    /// <summary>
    ///     Track command
    /// </summary>
    public class TrackCommand : Command<TrackOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(TrackOptions options)
        {
            await using var issues = Initialize(options);
            options.Tracked = TrackedIssue.None;
            if (!string.IsNullOrEmpty(options.Key))
            {
                var find = await issues.FindAsync(i => i.Key.ToString() == options.Key)
                    .FirstOrDefaultAsync();

                if (find != null)
                {
                    options.Tracked.Key = find.Key;
                    options.Tracked.Started = DateTime.Now;
                }
            }

            options.Tracked.Save(Path.Combine(options.Path, options.Name, options.Tracking), Logger);
            Console.WriteLine($"Tacking Issue '{options.Tracked.Key}'");
        }
    }
}