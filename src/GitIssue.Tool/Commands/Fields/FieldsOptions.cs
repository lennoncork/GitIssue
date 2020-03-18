using CommandLine;

namespace GitIssue.Util.Commands.Fields
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Fields), HelpText = "Shows the fields available")]
    public class FieldsOptions : Options
    {
    }
#pragma warning restore 1591
}