using CommandLine;
using GitIssue.Issues;
using GitIssue.Tool.Commands.Track;

namespace GitIssue.Tool.Commands.Create
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Changes), HelpText = "Shows the change log")]
    public class ChangesOptions : Options
    {
        
    }
#pragma warning restore 1591
}