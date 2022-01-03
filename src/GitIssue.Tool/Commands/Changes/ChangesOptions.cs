using CommandLine;

namespace GitIssue.Tool.Commands.Changes
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Changes), HelpText = "Shows the change log")]
    public class ChangesOptions : Options
    {

    }
#pragma warning restore 1591
}