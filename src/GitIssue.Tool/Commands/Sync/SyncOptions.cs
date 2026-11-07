using CommandLine;

namespace GitIssue.Tool.Commands.Sync
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Sync), HelpText = "Imports issues from a JSON file")]
    public class SyncOptions : Options
    {
        [Value(0, MetaName = "File", HelpText = "The JSON file to import", Required = false)]
        public string Import { get; set; } = "import.json";
    }
#pragma warning restore 1591
}