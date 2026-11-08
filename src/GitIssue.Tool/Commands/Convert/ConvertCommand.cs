using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Issues.Markdown;
using GitIssue.Validation;
using Serilog;

namespace GitIssue.Tool.Commands.Convert
{
    /// <summary>
    ///     Convert command
    /// </summary>
    public class ConvertCommand : Command<ConvertOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public ConvertCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(ConvertOptions options)
        {
            if (!Enum.TryParse(options.To, true, out IssueStorage target))
            {
                this.logger.Error($"Storage '{options.To}' is not Json or Markdown.");
                this.ExitCode = 1;
                return;
            }

            if (this.manager.Configuration is not IssueConfiguration configuration)
            {
                this.logger.Error("Issue configuration cannot be updated.");
                this.ExitCode = 1;
                return;
            }

            if (configuration.Storage == target)
            {
                Console.WriteLine($"Issue storage is already {target}.");
                return;
            }

            List<Issue> issues = new List<Issue>();
            await foreach (IIssue issue in this.manager.FindAsync(candidate => true))
            {
                if (issue is Issue concrete)
                {
                    issues.Add(concrete);
                }
            }

            foreach (Issue issue in issues)
            {
                string jsonPath = JsonIssue.GetJsonFile(issue.Root);
                string markdownPath = MarkdownIssue.GetMarkdownFile(issue.Root);
                if (File.Exists(jsonPath) && File.Exists(markdownPath))
                {
                    this.logger.Error($"Issue \"{issue.Key}\" has both JSON and Markdown files.");
                    this.ExitCode = 1;
                    return;
                }

                IReadOnlyList<IssueValidationError> errors = IssueValidator.ValidateFields(issue, configuration.Fields);
                if (errors.Count > 0)
                {
                    throw new IssueValidationException(issue.Key, errors);
                }
            }

            foreach (Issue issue in issues)
            {
                if (target == IssueStorage.Markdown)
                {
                    string markdown = MarkdownIssueFormat.Write(issue, configuration.Fields);
                    MarkdownRead read = MarkdownIssueFormat.Read(markdown, issue.Key, configuration.Fields);
                    if (read.Errors.Count > 0)
                    {
                        throw new IssueValidationException(issue.Key, read.Errors);
                    }

                    await Paths.WriteAllTextAsync(MarkdownIssue.GetMarkdownFile(issue.Root), markdown);
                }
                else if (issue is IJsonIssue jsonIssue)
                {
                    await jsonIssue.SaveAsJsonAsync(JsonIssue.GetJsonFile(issue.Root));
                }
            }

            foreach (Issue issue in issues)
            {
                string path = target == IssueStorage.Markdown
                    ? JsonIssue.GetJsonFile(issue.Root)
                    : MarkdownIssue.GetMarkdownFile(issue.Root);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            configuration.Storage = target;
            configuration.Save(this.manager.Root.ConfigFile);
            Console.WriteLine($"Converted {issues.Count} {(issues.Count == 1 ? "issue" : "issues")} to {target}.");
        }
    }
}