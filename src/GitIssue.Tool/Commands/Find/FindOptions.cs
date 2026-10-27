using CommandLine;

namespace GitIssue.Tool.Commands.Find
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Find), HelpText = "Finds an existing issue")]
    public class FindOptions : Options
    {
        [Option("Format", HelpText = "The format to output in", Required = false)]
        public string Format { get; set; } = "%Key %Title %Delta";

        [Value(0, MetaName = "Predicate", HelpText = "Matches title=, description=, author=, label=, or text=. The value is the remainder after the first =. An empty predicate matches every issue.", Required = false)]
        public string Linq { get; set; } = string.Empty;

        [Option("LinqName", HelpText = "Ignored. Predicates match issue fields directly and do not bind a variable name.", Required = false)]
        public string LinqName { get; set; } = "i";
    }
#pragma warning restore 1591
}