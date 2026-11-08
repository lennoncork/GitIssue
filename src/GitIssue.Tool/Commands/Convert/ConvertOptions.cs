using CommandLine;

namespace GitIssue.Tool.Commands.Convert
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Convert), HelpText = "Converts issue storage between JSON and Markdown")]
    public class ConvertOptions : Options
    {
        [Option("to", Required = true, HelpText = "Target storage: Json or Markdown")]
        public string To { get; set; } = string.Empty;
    }
#pragma warning restore 1591
}