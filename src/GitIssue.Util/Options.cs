using System;
using CommandLine;
using GitIssue.Keys;

namespace GitIssue.Util
{
#pragma warning disable 1591

    public class Options
    {
        [Option("path", Required = false, HelpText = "The path to the working directory")]
        public string Path { get; set; } = Environment.CurrentDirectory;

        [Option("name", Required = false, HelpText = "The name of the issues folder")]
        public string Name { get; set; } = ".issues";

        [Option("tracking", Required = false, HelpText = "The tracking file to use")]
        public string Tracking { get; set; } = "tracking.json";
    }

    public class KeyOptions: Options
    {
        private string key = string.Empty;

        /// <summary>
        /// Gets or sets the traced issue
        /// </summary>
        public TrackedIssue Tracked { get; set; }

        [Value(1, HelpText = "The issue key", Required = false)]
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    return this.Tracked?.Key.ToString() ?? IssueKey.None.ToString();
                }
                return key;
            }
            set => this.key = value;
        }
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
    public class DeleteOptions : KeyOptions
    {
    }

    [Verb(nameof(Command.Track), HelpText = "Tracks an existing issue")]
    public class TrackOptions : KeyOptions
    {
    }

    [Verb(nameof(Command.Find), HelpText = "Finds an existing issue")]
    public class FindOptions : Options
    {
        [Option("LinqName", HelpText = "The name of the issue in the linq expression", Required = false)]
        public string LinqName { get; set; } = "i";

        [Option("Format", HelpText = "The format to output in", Required = false)]
        public string Format { get; set; } = "%Key: %Title";

        [Value(1, HelpText = "The LINQ expression to use when matching", Required = false)]
        public string Linq { get; set; } = "i => true";
    }

    [Verb(nameof(Command.Show), HelpText = "Shows the details of an issue")]
    public class ShowOptions : KeyOptions
    {
    }

    [Verb(nameof(Command.Edit), HelpText = "Edits fields of an existing issue")]
    public class EditOptions : KeyOptions
    {
        [Value(2, HelpText = "The field to edit", Required = false)]
        public string Field { get; set; } = string.Empty;

        [Value(3, HelpText = "The text to update the field with", Required = false)]
        public string Update { get; set; } = string.Empty;
    }

#pragma warning restore 1591
}