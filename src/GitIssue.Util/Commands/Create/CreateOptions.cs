using CommandLine;

namespace GitIssue.Util.Commands.Create
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Create), HelpText = "Creates a new issue")]
    public class CreateOptions : Options
    {
        [Value(1, MetaName = "Title", HelpText = "The issue title", Required = true)]
        public string Title { get; set; }

        [Value(2, MetaName = "Description", HelpText = "The issue description", Required = false)]
        public string Description { get; set; } = "";
    }
#pragma warning restore 1591
}