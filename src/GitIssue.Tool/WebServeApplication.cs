using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Services;

namespace GitIssue.Tool
{
    /// <summary>
    ///     Runs <c>gitissue web serve</c>
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
            return WebServeApplication.Execute(args, output, error, AppContext.BaseDirectory);
        }

        internal static int Execute(string[] args, TextWriter output, TextWriter error, string hostDirectory)
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

            string? hostAssembly = WebServeApplication.findHostAssembly(hostDirectory);
            if (hostAssembly is null)
            {
                error.WriteLine("The GitIssue web host was not found next to the tool.");
                return 1;
            }

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "dotnet",
                UseShellExecute = false,
            };
            start.ArgumentList.Add("exec");
            start.ArgumentList.Add(hostAssembly);
            start.ArgumentList.Add("--path");
            start.ArgumentList.Add(request.RepositoryPath);
            start.ArgumentList.Add("--name");
            start.ArgumentList.Add(request.IssueFolderName);
            start.ArgumentList.Add("--host");
            start.ArgumentList.Add(request.Host);
            start.ArgumentList.Add("--port");
            start.ArgumentList.Add(request.Port.ToString());
            if (!string.IsNullOrEmpty(request.Token))
            {
                start.ArgumentList.Add("--token");
                start.ArgumentList.Add(request.Token);
            }

            using Process process = new Process
            {
                StartInfo = start,
            };
            process.Start();
            using CancellationTokenSource cancel = new CancellationTokenSource();
            ConsoleCancelEventHandler cancelled = (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancel.Cancel();
                WebServeApplication.stop(process);
            };
            Console.CancelKeyPress += cancelled;
            try
            {
                if (request.OpenBrowser && WebServeApplication.waitForHealth(request, cancel.Token))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = request.Url,
                        UseShellExecute = true,
                    });
                }

                process.WaitForExit();
                return process.HasExited ? process.ExitCode : 1;
            }
            finally
            {
                Console.CancelKeyPress -= cancelled;
                WebServeApplication.stop(process);
            }
        }

        private static string? findHostAssembly(string hostDirectory)
        {
            string candidate = Path.Combine(hostDirectory, "GitIssue.Services.Web.dll");
            return File.Exists(candidate) ? candidate : null;
        }

        private static bool waitForHealth(WebServeRequest request, CancellationToken cancellationToken)
        {
            using HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(1),
            };
            if (!string.IsNullOrEmpty(request.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);
            }

            for (int attempt = 0; (attempt < 50) && !cancellationToken.IsCancellationRequested; attempt++)
            {
                try
                {
                    using HttpResponseMessage response = client.GetAsync($"{request.Url}/health", cancellationToken)
                        .GetAwaiter()
                        .GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }

            return false;
        }

        private static void stop(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception)
            {
                // The process already exited.
            }
        }
    }
}
