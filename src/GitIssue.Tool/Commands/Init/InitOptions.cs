using CommandLine;

namespace GitIssue.Tool.Commands.Init
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Init), HelpText = "Initializes the issue repository")]
    public class InitOptions : Options
    {
        [Value(0, MetaName = "Issue Type", HelpText = "The type of issues that will be available", Required = false)]
        public string IssueType { get; set; } = string.Empty;

        [Option("storage", HelpText = "Issue storage format: Markdown or Json. Defaults to Markdown", Required = false)]
        public string Storage { get; set; } = "Markdown";

        [Option("issue-type", HelpText = "Issue type token used by {type}; ASCII letters and digits only. Defaults to ISSUE", Required = false)]
        public string IssueTypeName { get; set; } = "ISSUE";

        [Option("key-provider", HelpText = "Issue key provider: Date, Name, or Template. Defaults to Date", Required = false)]
        public string KeyProvider { get; set; } = "Date";

        [Option("key-template", HelpText = "Issue key template, for example {type}-{created}-{index}-{uid}-{slug}", Required = false)]
        public string KeyTemplate { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}
