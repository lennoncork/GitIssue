using System;

namespace GitIssue.Exceptions
{
    /// <summary>
    /// Exception thrown when an issue repository cannot be found
    /// </summary>
    public class RepositoryNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryNotFoundException"/> class
        /// </summary>
        /// <param name="message">the exception message</param>
        public RepositoryNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryNotFoundException"/> class
        /// </summary>
        /// <param name="message">the exception message</param>
        /// <param name="ex">the internal exception</param>
        public RepositoryNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
