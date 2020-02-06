using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace GitIssue.Util
{
#pragma warning disable 1591

    public class Options
    {
        [Option("path", Required = false, HelpText = "The working path")]
        public string Path { get; set; } = Environment.CurrentDirectory;

        [Option("name", Required = false, HelpText = "The issue type")]
        public string Type { get; set; } = ".issues";
    }

    [Verb(nameof(Command.Init), HelpText = "Initializes the issue repository")]
    public class InitOptions : Options
    {
    }

    [Verb(nameof(Command.Create), HelpText = "Creates a new issue")]
    public class CreateOptions : Options
    {
        [Value(1, HelpText = "The issue title", Required = true)]
        public string Title { get; set; }

        [Value(1, HelpText = "The issue description", Required = false)]
        public string Description { get; set; } = "";
    }

    [Verb(nameof(Command.Delete), HelpText = "Deletes an existing issue")]
    public class DeleteOptions : Options
    {
        [Value(1, HelpText = "The issue key", Required = true)]
        public string Key { get; set; }
    }

    [Verb(nameof(Command.Find), HelpText = "Finds an existing issue")]
    public class FindOptions : Options
    {
        [Option("LinqName", HelpText = "The name of the issue in the linq expression", Required = false)]
        public string Name { get; set; } = "i";

        [Option("Format", HelpText = "The format to output in", Required = false)]
        public string Format { get; set; } = "%Key: %Title";

        [Value(1, HelpText = "The LINQ expression to use when matching", Required = false)]
        public string Linq { get; set; } = "i => true";

    }

    [Verb(nameof(Command.Show), HelpText = "Shows the details of an issue")]
    public class ShowOptions : Options
    {
        [Value(1, HelpText = "The issue key to use", Required = false)]
        public string Key { get; set; } = String.Empty;
    }

    [Verb(nameof(Command.Edit), HelpText = "Edits fields of an existing issue")]
    public class EditOptions : Options
    {
        [Value(1, HelpText = "The issue to edit", Required = true)]
        public string Key { get; set; } = String.Empty;

        [Value(2, HelpText = "The field to edit", Required = true)]
        public string Field { get; set; } = String.Empty;

        [Value(3, HelpText = "The text to update the field with", Required = true)]
        public string Update { get; set; } = String.Empty;
    }

#pragma warning restore 1591
}
