using CommandLine;

namespace GitIssue.Tool.Commands.Sync
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Export), HelpText = "Exports all issues into a JSON file")]
    public class SyncOptions : Options
    {
        [Value(1, MetaName = "Import Name", HelpText = "The name of the sync to use", Required = false)]
        public string Import { get; set; } = "disk";
    }
#pragma warning restore 1591
}