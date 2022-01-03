using CommandLine;

namespace GitIssue.Tool.Commands.Import
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Import), HelpText = "Imports issues from a JSON or CSV file")]
    public class ImportOptions : Options
    {
        [Value(1, MetaName = "File", HelpText = "The file to import issues from", Required = false)]
        public string Import { get; set; } = "export.json";

        [Option("Update", HelpText = "Enables updating of existing issues", Required = false)]
        public bool Overwrite { get; set; } = false;

        [Option("Separator", HelpText = "The seperator to use when importing from CSV", Required = false)]
        public string Separator { get; set; } = "; ";
    }
#pragma warning restore 1591
}