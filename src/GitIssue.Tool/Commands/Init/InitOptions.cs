using CommandLine;

namespace GitIssue.Tool.Commands.Init
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Init), HelpText = "Initializes the issue repository")]
    public class InitOptions : Options
    {
        [Value(1, MetaName = "Issue Type", HelpText = "The type of issues that will be available", Required = false)]
        public string IssueType { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}