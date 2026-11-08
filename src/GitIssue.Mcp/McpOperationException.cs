using System;

namespace GitIssue.Mcp
{
    /// <summary>
    ///     An MCP operation could not be completed
    /// </summary>
    public class McpOperationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="McpOperationException" /> class
        /// </summary>
        /// <param name="message">the error message</param>
        public McpOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="McpOperationException" /> class
        /// </summary>
        /// <param name="message">the error message</param>
        /// <param name="inner">the exception that caused this failure</param>
        public McpOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
