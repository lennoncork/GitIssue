using CommandLine;

namespace GitIssue.Tool.Commands.Add
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Add), HelpText = "Add a value to an existing field on an existing issue")]
    public class AddOptions : EditorOptions
    {
        [Value(2, MetaName = "Field Key", HelpText = "The field to edit", Required = false)]
        public string Field { get; set; } = string.Empty;

        [Value(3, MetaName = "Value", HelpText = "The text to update the field with", Required = false)]
        public string Add { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}