using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
            List<IssueKey> issues = new List<IssueKey>();
            if (string.IsNullOrEmpty(options.Issue))
            {
                issues.AddRange(this.manager.KeyProvider.Keys);
            }
            else
            {
                string key = this.resolveKey(options);
                IssueKey[] matches = this.manager.KeyProvider.Keys.Where(candidate => candidate.ToString() == key).ToArray();
                if (matches.Length == 0)
                {
                    this.logger.Error($"Issue \"{key}\" not found");
                    this.ExitCode = 1;
                    return;
                }

                issues.Add(matches[0]);
            }

            int invalid = 0;
            foreach (IssueKey issue in issues)
            {
                IReadOnlyList<IssueValidationError> errors = await this.validateAsync(issue);
                if (errors.Count == 0)
                {
                    continue;
                }

                invalid++;
                Console.WriteLine(issue);
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

        private async Task<IReadOnlyList<IssueValidationError>> validateAsync(IssueKey issue)
        {
            IssueRoot root = new IssueRoot(this.manager.Root, issue, this.manager.KeyProvider.GetIssuePath(issue));
            string path = this.manager.Configuration.Storage == IssueStorage.Markdown
                ? MarkdownIssue.GetMarkdownFile(root)
                : JsonIssue.GetJsonFile(root);
            if (!File.Exists(path))
            {
                return new[]
                {
                    new IssueValidationError(issue, FieldKey.None, null, "issue file is missing."),
                };
            }

            string type;
            try
            {
                type = await IssueTypeReader.ReadAsync(root, this.manager.Configuration);
            }
            catch (Exception exception) when (exception is IOException or ArgumentException or JsonException)
            {
                return new[]
                {
                    new IssueValidationError(issue, FieldKey.Create("IssueType"), null, exception.Message),
                };
            }

            var schema = this.manager.Configuration.GetFields(type);
            string text = await File.ReadAllTextAsync(path);
            if (this.manager.Configuration.Storage == IssueStorage.Markdown)
            {
                return MarkdownIssueFormat.Read(text, issue, schema).Errors;
            }

            return IssueValidator.ValidateJson(text, issue, schema);
        }
    }
}
