using CommandLine;

namespace GitIssue.Util
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Init), HelpText = "Initializes the issue repository")]
    public class InitOptions : Options
    {
    }
#pragma warning restore 1591
}