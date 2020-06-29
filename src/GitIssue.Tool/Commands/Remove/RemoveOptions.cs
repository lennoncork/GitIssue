using CommandLine;

namespace GitIssue.Util.Commands.Remove
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Remove), HelpText = "Removes a value from an existing field on an existing issue")]
    public class RemoveOptions : KeyOptions
    {
        [Value(2, MetaName = "Field Key", HelpText = "The field to remove an item from", Required = false)]
        public string Field { get; set; } = string.Empty;

        [Value(3, MetaName = "Value", HelpText = "The item to remove", Required = false)]
        public string Remove { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}