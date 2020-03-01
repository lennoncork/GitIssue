using CommandLine;
using GitIssue.Issues;
using GitIssue.Util.Commands.Track;

namespace GitIssue.Util.Commands.Create
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Create), HelpText = "Creates a new issue")]
    public class CreateOptions : Options, ITrackedOptions
    {
        [Option("Track", HelpText = "Track the new issue, even if another issue is already tracked", Required = false)]
        public bool Track { get; set; } = false;

        [Value(1, MetaName = "Title", HelpText = "The issue title", Required = true)]
        public string Title { get; set; } = string.Empty;

        [Value(2, MetaName = "Description", HelpText = "The issue description", Required = false)]
        public string Description { get; set; } = string.Empty;

        public TrackedIssue Tracked { get; set; } = TrackedIssue.None;
    }
#pragma warning restore 1591
}