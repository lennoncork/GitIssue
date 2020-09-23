using CommandLine;

namespace GitIssue.Tool.Commands.Commit
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Commit), HelpText = "Commits the issues")]
    public class CommitOptions : KeyOptions
    {
    }
#pragma warning restore 1591
}