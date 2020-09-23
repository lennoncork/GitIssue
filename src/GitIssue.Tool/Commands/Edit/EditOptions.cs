using CommandLine;

namespace GitIssue.Tool.Commands.Edit
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Edit), HelpText = "Edits fields of an existing issue")]
    public class EditOptions : EditorOptions
    {
        [Value(2, MetaName = "Field Key", HelpText = "The field to edit", Required = false)]
        public string Field { get; set; } = string.Empty;

        [Value(3, MetaName = "Value", HelpText = "The text to update the field with", Required = false)]
        public string Update { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}