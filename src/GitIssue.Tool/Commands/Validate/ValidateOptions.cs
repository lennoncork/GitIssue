using CommandLine;

namespace GitIssue.Tool.Commands.Validate
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Validate), HelpText = "Validates issue documents against the configured fields")]
    public class ValidateOptions : TrackedOptions
    {
        [Value(0, MetaName = "Issue Key", HelpText = "The issue to validate. Omit to validate every issue. Use '.', 'T', or 'Tracked' for the tracked issue.", Required = false)]
        public string? Issue { get; set; }
    }
#pragma warning restore 1591
}
