using CommandLine;

namespace GitIssue.Tool.Commands.Create
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Create), HelpText = "Creates a new issue")]
    public class CreateOptions : TrackedOptions
    {
        [Option("arguments", HelpText = "Any additional arguments to give to the editor", Required = false)]
        public string Arguments { get; set; } = "-pound_comment -syntax git-commit";

        [Value(1, MetaName = "Description", HelpText = "The issue description", Required = false)]
        public string Description { get; set; } = string.Empty;

        [Option("editor", HelpText = "The editor to use", Required = false)]
        public string Editor { get; set; } = "joe";

        [Value(0, MetaName = "Title", HelpText = "The issue title", Required = false)]
        public string Title { get; set; } = string.Empty;

        [Option("Track", HelpText = "Track the new issue, even if another issue is already tracked", Required = false)]
        public bool Track { get; set; } = false;
    }
#pragma warning restore 1591
}