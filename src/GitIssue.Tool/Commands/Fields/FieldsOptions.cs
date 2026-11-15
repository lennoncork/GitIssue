using CommandLine;

namespace GitIssue.Tool.Commands.Fields
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Fields), HelpText = "Shows the list of available fields")]
    public class FieldsOptions : Options
    {
        [Option("type", HelpText = "Configured issue type. Defaults to the repository's default type", Required = false)]
        public string IssueType { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}
