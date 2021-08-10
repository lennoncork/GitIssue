using CommandLine;

namespace GitIssue.Tool.Commands.Export
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Export), HelpText = "Exports all issues into a JSON file")]
    public class ExportOptions : Options
    {
        [Value(1, MetaName = "File", HelpText = "The file to export issues to", Required = false)]
        public string Export { get; set; } = "export.json";

        [Option("Overwrite", HelpText = "Enable overwrite of the export file", Required = false)]
        public bool Overwrite { get; set; } = false;

        [Option("Separator", HelpText = "The seperator to use when exporting CSV", Required = false)]
        public string Separator { get; set; } = "; ";
    }
#pragma warning restore 1591
}