using CommandLine;

namespace GitIssue.Tool.Commands.Comment
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Comment), HelpText = "Add a new comment to an existing issue")]
    public class CommentOptions : EditorOptions
    {
        [Value(2, MetaName = "Comment", HelpText = "The comment to add to the issue", Required = false)]
        public string Comment { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}