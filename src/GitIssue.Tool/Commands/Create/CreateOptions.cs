using CommandLine;

namespace GitIssue.Tool.Commands.Create
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Create), HelpText = "Creates a new issue")]
    public class CreateOptions : EditorOptions, ITrackedOptions
    {
        [Value(2, MetaName = "Description", HelpText = "The issue description", Required = false)]
        public string Description { get; set; } = string.Empty;

        [Value(1, MetaName = "Title", HelpText = "The issue title", Required = false)]
        public string Title { get; set; } = string.Empty;

        [Option("Track", HelpText = "Track the new issue, even if another issue is already tracked", Required = false)]
        public bool Track { get; set; } = false;
    }
#pragma warning restore 1591
}