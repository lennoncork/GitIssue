using System.IO;
using GitIssue.Services.Mcp;

namespace GitIssue.Tool.Commands.Web
{
    /// <summary>
    ///     The <c>gitissue web</c> command
    /// </summary>
    public static class WebCommand
    {
        /// <summary>
        ///     Parses web arguments and runs the website in this process
        /// </summary>
        /// <param name="args">the process arguments, including the leading web token</param>
        /// <param name="output">the standard output writer</param>
        /// <param name="error">the standard error writer</param>
        /// <returns>the process exit code</returns>
        public static int Execute(string[] args, TextWriter output, TextWriter error)
        {
            return WebServeApplication.Execute(args, output, error);
        }
    }
}
