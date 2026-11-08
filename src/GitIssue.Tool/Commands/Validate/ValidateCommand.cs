using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Issues.Markdown;
using GitIssue.Validation;
using Serilog;

namespace GitIssue.Tool.Commands.Validate
{
    /// <summary>
    ///     Validate command
    /// </summary>
    public class ValidateCommand : Command<ValidateOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public ValidateCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(ValidateOptions options)
        {
            List<IIssue> issues = new List<IIssue>();
            if (string.IsNullOrEmpty(options.Issue))
            {
                await foreach (IIssue issue in this.manager.FindAsync(candidate => true))
                {
                    issues.Add(issue);
                }
            }
            else
            {
                string key = this.resolveKey(options);
                IIssue? issue = await this.manager
                    .FindAsync(candidate => candidate.Key.ToString() == key)
                    .FirstOrDefaultAsync();
                if (issue == null)
                {
                    this.logger.Error($"Issue \"{key}\" not found");
                    this.ExitCode = 1;
                    return;
                }

                issues.Add(issue);
            }

            int invalid = 0;
            foreach (IIssue issue in issues)
            {
                IReadOnlyList<IssueValidationError> errors = await this.validateAsync(issue);
                if (errors.Count == 0)
                {
                    continue;
                }

                invalid++;
                Console.WriteLine(issue.Key);
                foreach (IssueValidationError error in errors)
                {
                    Console.WriteLine($"  {error}");
                }

                Console.WriteLine();
            }

            Console.WriteLine($"{issues.Count} {(issues.Count == 1 ? "issue" : "issues")} checked, {invalid} invalid.");
            if (invalid > 0)
            {
                this.ExitCode = 1;
            }
        }

        private string resolveKey(ValidateOptions options)
        {
            if ((options.Issue == ".") || (options.Issue == "T") || (options.Issue == "Tracked"))
            {
                return options.Tracked?.Key.ToString() ?? IssueKey.None.ToString();
            }

            return options.Issue ?? string.Empty;
        }

        private async Task<IReadOnlyList<IssueValidationError>> validateAsync(IIssue issue)
        {
            if (issue is not Issue concrete)
            {
                return IssueValidator.ValidateFields(issue, this.manager.Configuration.Fields);
            }

            if (this.manager.Configuration.Storage == IssueStorage.Markdown)
            {
                string path = MarkdownIssue.GetMarkdownFile(concrete.Root);
                if (!File.Exists(path))
                {
                    return new[]
                    {
                        new IssueValidationError(issue.Key, FieldKey.None, null, "markdown issue file is missing."),
                    };
                }

                string text = await File.ReadAllTextAsync(path);
                return MarkdownIssueFormat.Read(text, concrete.Root.Key, this.manager.Configuration.Fields).Errors;
            }

            string jsonPath = JsonIssue.GetJsonFile(concrete.Root);
            if (!File.Exists(jsonPath))
            {
                return new[]
                {
                    new IssueValidationError(issue.Key, FieldKey.None, null, "json issue file is missing."),
                };
            }

            string json = await File.ReadAllTextAsync(jsonPath);
            return IssueValidator.ValidateJson(json, concrete.Root.Key, this.manager.Configuration.Fields);
        }
    }
}