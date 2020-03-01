using System;
using System.IO;

namespace GitIssue
{
    /// <summary>
    ///     Exception thrown when an issue cannot be found
    /// </summary>
    public class SafeResultException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResultException" /> class
        /// </summary>
        /// <param name="message">the exception message</param>
        public SafeResultException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResultException" /> class
        /// </summary>
        /// <param name="message">the exception message</param>
        /// <param name="ex">the internal exception</param>
        public SafeResultException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}