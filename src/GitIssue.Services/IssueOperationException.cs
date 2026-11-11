using System;

namespace GitIssue.Services
{
    /// <summary>
    ///     An issue operation could not be completed
    /// </summary>
    public class IssueOperationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueOperationException" /> class
        /// </summary>
        /// <param name="message">the error message</param>
        public IssueOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueOperationException" /> class
        /// </summary>
        /// <param name="message">the error message</param>
        /// <param name="inner">the exception that caused this failure</param>
        public IssueOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
