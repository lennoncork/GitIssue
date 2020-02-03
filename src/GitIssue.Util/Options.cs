using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace GitIssue.Util
{
    /// <summary>
    ///     Command Line Options
    /// </summary>
    public class Options
    {
        /// <summary>
        ///     The version type to deserialize
        /// </summary>
        [Option("path", Required = false, HelpText = "The working path")]
        public string Path { get; set; } = Environment.CurrentDirectory;
    }

    /// <summary>
    /// Initialization Options
    /// </summary>
    [Verb(nameof(Command.Init), HelpText = "Initializes the issue repository")]
    public class InitOptions : Options
    {
    }

    /// <summary>
    /// Issue Creation Options
    /// </summary>
    [Verb(nameof(Command.Create), HelpText = "Creates a new issue")]
    public class CreateOptions : Options
    {
        [Value(1, HelpText = "The issue title", Required = true)]
        public string Title { get; set; }

        [Value(1, HelpText = "The issue description", Required = false)]
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Issue Deletion Options
    /// </summary>
    [Verb(nameof(Command.Delete), HelpText = "Deletes an existing issue")]
    public class DeleteOptions : Options
    {
        [Value(1, HelpText = "The issue key", Required = true)]
        public string Key { get; set; }
    }

    /// <summary>
    /// Issue Find Options
    /// </summary>
    [Verb(nameof(Command.Find), HelpText = "Finds an existing issue")]
    public class FindOptions : Options
    {
        [Option("LinqName", HelpText = "The name of the issue in the linq expression", Required = false)]
        public string Name { get; set; } = "i";

        [Value(1, HelpText = "The LINQ expression to use when matching", Required = false)]
        public string Linq { get; set; } = "i => true";
    }

    /// <summary>
    /// Issue Showing Options
    /// </summary>
    [Verb(nameof(Command.Show), HelpText = "Shows the details of an issue")]
    public class ShowOptions : Options
    {
        /// <summary>
        /// The issue key to show
        /// </summary>
        [Value(1, HelpText = "The issue key to use", Required = false)]
        public string Key { get; set; } = String.Empty;
    }

    /// <summary>
    /// Issue Showing Options
    /// </summary>
    [Verb(nameof(Command.Edit), HelpText = "Edits fields of an existing issue")]
    public class EditOptions : Options
    {
        /// <summary>
        /// The issue key to show
        /// </summary>
        [Value(1, HelpText = "The issue to edit", Required = true)]
        public string Key { get; set; } = String.Empty;

        /// <summary>
        /// The issue key to show
        /// </summary>
        [Value(2, HelpText = "The field to edit", Required = true)]
        public string Field { get; set; } = String.Empty;

        /// <summary>
        /// The issue key to show
        /// </summary>
        [Value(3, HelpText = "The text to update the field with", Required = true)]
        public string Update { get; set; } = String.Empty;
    }
}
