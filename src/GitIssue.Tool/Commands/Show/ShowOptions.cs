using CommandLine;

namespace GitIssue.Util.Commands.Show
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Show), HelpText = "Shows the details of an issue")]
    public class ShowOptions : KeyOptions
    {
        [Value(1, MetaName = "Show What", HelpText = "Show issues", Required = false)]
        public ShowSubCommand Show { get; set; } = ShowSubCommand.Tracked;
    }
#pragma warning restore 1591
}