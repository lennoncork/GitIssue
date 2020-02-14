﻿using CommandLine;

namespace GitIssue.Util
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Find), HelpText = "Finds an existing issue")]
    public class FindOptions : Options
    {
        [Option("LinqName", HelpText = "The name of the issue in the linq expression", Required = false)]
        public string LinqName { get; set; } = "i";

        [Option("Format", HelpText = "The format to output in", Required = false)]
        public string Format { get; set; } = "%Key: %Title";

        [Value(1, HelpText = "The LINQ expression to use when matching", Required = false)]
        public string Linq { get; set; } = "i => true";
    }
#pragma warning restore 1591
}