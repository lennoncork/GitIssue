using System;
using System.Collections.Generic;
using CommandLine;
using GitIssue.Issues;

namespace GitIssue.Tool.Commands
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

    public interface ITrackedOptions
    {
        public string Path { get; set; }

        public string Name { get; set; }

        TrackedIssue Tracked { get; set; }

        public string Tracking { get; set; }
    }

    public class KeyOptions : Options, ITrackedOptions
    {
        private static readonly HashSet<string?> tracking = new HashSet<string?>
        {
            null,
            string.Empty,
            ".",
            "T",
            "Tracked"
        };

        private string key = string.Empty;

        [Value(1, MetaName = "Issue Key", HelpText = "The issue key (use '.', 'T', or 'Tracked' to refernce the tracked issue)", Required = false)]
        public string Key
        {
            get
            {
                if (tracking.Contains(key))
                    return Tracked?.Key.ToString() ?? IssueKey.None.ToString();
                return key;
            }
            set => key = value;
        }

        /// <summary>
        ///     Gets or sets the traced issue
        /// </summary>
        public TrackedIssue Tracked { get; set; } = TrackedIssue.None;
    }

    public class EditorOptions : KeyOptions
    {
        [Option("editor", HelpText = "The editor to use", Required = false)]
        public string Editor { get; set; } = "joe";

        [Option("arguments", HelpText = "Any additional arguments to give to the editor", Required = false)]
        public string Arguments { get; set; } = "-pound_comment -syntax git-commit";
    }
#pragma warning restore 1591
}