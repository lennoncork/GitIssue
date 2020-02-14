using CommandLine;

namespace GitIssue.Util
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Delete), HelpText = "Deletes an existing issue")]
    public class DeleteOptions : KeyOptions
    {
    }
#pragma warning restore 1591
}