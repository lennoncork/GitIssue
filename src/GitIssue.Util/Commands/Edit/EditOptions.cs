using CommandLine;

namespace GitIssue.Util
{

#pragma warning disable 1591
    [Verb(nameof(CommandType.Edit), HelpText = "Edits fields of an existing issue")]
    public class EditOptions : KeyOptions
    {
        [Option("editor", HelpText = "The editor to use", Required = false)]
        public string Editor { get; set; } = "joe";

        [Option("arguments", HelpText = "Any additional arguments to give to the editor", Required = false)]
        public string Arguments { get; set; } = "-pound_comment -syntax git-commit";

        [Value(2, HelpText = "The field to edit", Required = false)]
        public string Field { get; set; } = string.Empty;

        [Value(3, HelpText = "The text to update the field with", Required = false)]
        public string Update { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}