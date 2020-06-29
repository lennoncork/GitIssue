using CommandLine;

namespace GitIssue.Util.Commands.Fields
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Fields), HelpText = "Shows the list of available fields")]
    public class FieldsOptions : Options
    {
    }
#pragma warning restore 1591
}