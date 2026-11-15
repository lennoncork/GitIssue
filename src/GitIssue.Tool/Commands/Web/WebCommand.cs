using System;
using System.Threading.Tasks;
using GitIssue.Services;
using GitIssue.Services.Mcp;

namespace GitIssue.Tool.Commands.Web
{
    /// <summary>
    ///     The <c>gitissue web</c> command
    /// </summary>
    public class WebCommand : Command<WebOptions>
    {
        /// <inheritdoc />
        public override Task Exec(WebOptions options)
        {
            string action = string.IsNullOrWhiteSpace(options.Action) ? "serve" : options.Action;
            if (action.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(WebServeCommandLine.Usage);
                return Task.CompletedTask;
            }

            if (!action.Equals("serve", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"Unknown web action \"{action}\".");
                Console.Error.WriteLine(WebServeCommandLine.Usage);
                this.ExitCode = 1;
                return Task.CompletedTask;
            }

            if ((options.Port < 1) || (options.Port > 65535))
            {
                Console.Error.WriteLine("--port must be from 1 to 65535.");
                Console.Error.WriteLine(WebServeCommandLine.Usage);
                this.ExitCode = 1;
                return Task.CompletedTask;
            }

            if (!WebCommand.isLoopback(options.Host) && string.IsNullOrEmpty(options.Token))
            {
                Console.Error.WriteLine("--token is required when --host is not loopback.");
                Console.Error.WriteLine(WebServeCommandLine.Usage);
                this.ExitCode = 1;
                return Task.CompletedTask;
            }

            WebServeRequest request = new WebServeRequest
            {
                RepositoryPath = options.Path,
                IssueFolderName = options.Name,
                Host = options.Host,
                Port = options.Port,
                OpenBrowser = options.Open,
                Token = options.Token,
            };

            try
            {
                WebServeApplication.Run(request);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                this.ExitCode = 1;
            }

            return Task.CompletedTask;
        }

        private static bool isLoopback(string host)
        {
            return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("::1", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("[::1]", StringComparison.OrdinalIgnoreCase);
        }
    }
}
