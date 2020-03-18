using CommandLine;
using GitIssue.Issues;
using GitIssue.Util.Commands.Track;

namespace GitIssue.Util.Commands.Create
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Changes), HelpText = "Shows the change log")]
    public class ChangesOptions : Options
    {
        
    }
#pragma warning restore 1591
}