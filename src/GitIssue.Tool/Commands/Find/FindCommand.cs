using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Formatters;
using GitIssue.Issues;
using Serilog;

namespace GitIssue.Tool.Commands.Find
{
    /// <summary>
    ///     Fine options
    /// </summary>
    public class FindCommand : Command<FindOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public FindCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(FindOptions options)
        {
            Func<IIssue, bool> issueFilter = this.CreatePredicate(options.Linq);
            TerminalFormatter formatter = new TerminalFormatter(options.Format);
            IAsyncEnumerable<IIssue> find = this.manager.FindAsync(issue => issueFilter.Invoke(issue));
            await foreach (IIssue issue in find)
            {
                Console.WriteLine(issue.Format(formatter));
            }
        }

        private Func<IIssue, bool> CreatePredicate(string? query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return static issue => true;
            }

            int separator = query.IndexOf('=');
            if (separator > 0)
            {
                string field = query.Substring(0, separator);
                string value = query.Substring(separator + 1);
                switch (field)
                {
                    case "title":
                        return issue => string.Equals(issue.Title.ToString(), value, StringComparison.Ordinal);
                    case "description":
                        return issue => string.Equals(issue.Description.ToString(), value, StringComparison.Ordinal);
                    case "author":
                        return issue => string.Equals(issue.Author.ToString(), value, StringComparison.Ordinal);
                    case "label":
                        return issue => FindCommand.ContainsLabel(issue, value);
                    case "text":
                        return issue => FindCommand.ContainsText(issue, value);
                }
            }

            const string message = "Invalid find predicate. Expected an empty query or one of title=, description=, author=, label=, or text=.";
            this.logger.Error("Invalid find predicate {Predicate}. Expected an empty query or one of title=, description=, author=, label=, or text=.", query);
            throw new InvalidOperationException(message);
        }

        private static bool ContainsLabel(IIssue issue, string value)
        {
            if (!issue.TryGetValue(FieldKey.Create("Labels"), out IField? field) || field is not IArrayField arrayField)
            {
                return false;
            }

            object[]? labels = arrayField.Values;
            if (labels == null)
            {
                return false;
            }

            foreach (object label in labels)
            {
                if (string.Equals(label?.ToString(), value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsText(IIssue issue, string value)
        {
            string title = issue.Title.ToString() ?? string.Empty;
            string description = issue.Description.ToString() ?? string.Empty;
            return title.Contains(value, StringComparison.Ordinal)
                || description.Contains(value, StringComparison.Ordinal);
        }
    }
}
