using CommandLine;
using GitIssue.Issues;

namespace GitIssue.Tool.Commands.Show
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Show), HelpText = "Shows the details of an issue")]
    public class ShowOptions : TrackedOptions
    {
        public string Key => this.Tracked?.Key.ToString() ?? IssueKey.None.ToString();

        [Value(0, MetaName = "Show What", HelpText = "Show issues", Required = false)]
        public ShowSubCommand Show { get; set; } = ShowSubCommand.Tracked;
    }
#pragma warning restore 1591
}