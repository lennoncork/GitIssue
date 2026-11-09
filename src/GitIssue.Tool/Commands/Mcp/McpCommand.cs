using System.IO;
using GitIssue.Services.Mcp;

namespace GitIssue.Tool.Commands.Mcp
{
    /// <summary>
    ///     The <c>gitissue mcp</c> command
    /// </summary>
    public static class McpCommand
    {
        /// <summary>
        ///     Parses mcp arguments and runs the selected action
        /// </summary>
        /// <param name="args">the process arguments, including the leading mcp token</param>
        /// <param name="output">the standard output writer</param>
        /// <param name="error">the standard error writer</param>
        /// <returns>the process exit code</returns>
        public static int Execute(string[] args, TextWriter output, TextWriter error)
        {
            return McpApplication.Execute(args, output, error);
        }
    }
}
