using System;
using System.IO;

namespace GitIssue.Exceptions
{
    /// <summary>
    /// Exception thrown when an issue cannot be found
    /// </summary>
    public class IssueNotFoundException : IOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IssueNotFoundException"/> class
        /// </summary>
        /// <param name="message">the exception message</param>
        public IssueNotFoundException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueNotFoundException"/> class
        /// </summary>
        /// <param name="message">the exception message</param>
        /// <param name="ex">the internal exception</param>
        public IssueNotFoundException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
