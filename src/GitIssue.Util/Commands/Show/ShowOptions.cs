using CommandLine;

namespace GitIssue.Util
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Show), HelpText = "Shows the details of an issue")]
    public class ShowOptions : KeyOptions
    {
    }
#pragma warning restore 1591
}