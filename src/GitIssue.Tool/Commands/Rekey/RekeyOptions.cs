using CommandLine;

namespace GitIssue.Tool.Commands.Rekey
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Rekey), HelpText = "Regenerates issue keys and file names from the configured key template")]
    public class RekeyOptions : Options
    {
        [Option("dry-run", HelpText = "Show planned renames without writing files", Required = false, Default = false)]
        public bool DryRun { get; set; }
    }
#pragma warning restore 1591
}
