using System;
using System.Collections.Generic;
using System.Linq;
using GitIssue.Issues;
using GitIssue.Validation;

namespace GitIssue.Exceptions
{
    /// <summary>
    ///     Exception thrown when an issue fails field validation
    /// </summary>
    public class IssueValidationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueValidationException" /> class
        /// </summary>
        /// <param name="issue">the issue key</param>
        /// <param name="errors">the validation errors</param>
        public IssueValidationException(IssueKey issue, IReadOnlyList<IssueValidationError> errors)
            : base(IssueValidationException.format(issue, errors))
        {
            this.Issue = issue;
            this.Errors = errors;
        }

        /// <summary>
        ///     Gets the issue key
        /// </summary>
        public IssueKey Issue { get; }

        /// <summary>
        ///     Gets the validation errors
        /// </summary>
        public IReadOnlyList<IssueValidationError> Errors { get; }

        private static string format(IssueKey issue, IReadOnlyList<IssueValidationError> errors)
        {
            string details = string.Join(Environment.NewLine, errors.Select(error => error.ToString()));
            return $"Issue '{issue}' failed validation:{Environment.NewLine}{details}";
        }
    }
}
