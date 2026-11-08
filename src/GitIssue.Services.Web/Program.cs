using System.Threading.Tasks;
using GitIssue.Services;

namespace GitIssue.Services.Web
{
    /// <summary>
    ///     Entry point for the GitIssue web host
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     Runs the web host
        /// </summary>
        /// <param name="args">the host arguments</param>
        /// <returns>the process exit code</returns>
        public static async Task<int> Main(string[] args)
        {
            int prepared = Program.Prepare(args, System.Console.Out, System.Console.Error);
            if (prepared >= 0)
            {
                return prepared;
            }

            WebServeRequest request = WebServeCommandLine.ParseHost(args);
            await WebServerHost.Create(request).RunAsync();
            return 0;
        }

        /// <summary>
        ///     Handles help and invalid host arguments
        /// </summary>
        /// <param name="args">the host arguments</param>
        /// <param name="output">the standard output writer</param>
        /// <param name="error">the standard error writer</param>
        /// <returns>the exit code, or -1 when the host should run</returns>
        public static int Prepare(string[] args, System.IO.TextWriter output, System.IO.TextWriter error)
        {
            WebServeRequest request = WebServeCommandLine.ParseHost(args);
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

            return -1;
        }
    }
}
