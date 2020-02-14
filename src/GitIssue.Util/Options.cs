using System;
using CommandLine;
using GitIssue.Issues;

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

    public class KeyOptions : Options
    {
        private string key = string.Empty;

        /// <summary>
        ///     Gets or sets the traced issue
        /// </summary>
        public TrackedIssue Tracked { get; set; }

        [Value(1, HelpText = "The issue key", Required = false)]
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return Tracked?.Key.ToString() ?? IssueKey.None.ToString();
                return key;
            }
            set => key = value;
        }
    }


#pragma warning restore 1591
}