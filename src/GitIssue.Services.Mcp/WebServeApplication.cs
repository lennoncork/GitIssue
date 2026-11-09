using System;
using System.Diagnostics;
using System.IO;
using Exceptionable;
using GitIssue.Services;
using GitIssue.Services.Web;
using Microsoft.AspNetCore.Builder;

namespace GitIssue.Services.Mcp
{
    /// <summary>
    ///     Runs <c>gitissue web serve</c> in the current process
    /// </summary>
    public static class WebServeApplication
    {
        /// <summary>
        ///     Executes web arguments
        /// </summary>
        /// <param name="args">the process arguments, including the leading web token</param>
        /// <param name="output">the standard output writer</param>
        /// <param name="error">the standard error writer</param>
        /// <returns>the process exit code</returns>
        public static int Execute(string[] args, TextWriter output, TextWriter error)
        {
            WebServeRequest request = WebServeCommandLine.ParseCli(args);
            if (request.Help)
            {
                output.WriteLine(WebServeCommandLine.Usage);
                return 0;
            }

            if (!request.IsValid)
            {
                error.WriteLine(request.Error);
                error.WriteLine(WebServeCommandLine.Usage);
                return 1;
            }

            try
            {
                WebServeApplication.Run(request);
                return 0;
            }
            catch (Exception exception)
            {
                error.WriteLine(exception.Message);
                return 1;
            }
        }

        /// <summary>
        ///     Runs the website until the host shuts down
        /// </summary>
        /// <param name="request">the serve request</param>
        [Throws<IssueOperationException>]
        public static void Run(WebServeRequest request)
        {
            WebApplication application = WebServerHost.Create(request);
            if (request.OpenBrowser)
            {
                application.Lifetime.ApplicationStarted.Register(() => WebServeApplication.openBrowser(request.Url));
            }

            application.Run();
        }

        private static void openBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
            }
            catch (Exception)
            {
                // The website keeps running when the browser cannot be opened.
            }
        }
    }
}
