using System.Diagnostics;
using System.Threading.Tasks;

namespace GitIssue.Tool
{
    public class ProcessRunner : IProcessRunner
    {
        public Task<int> RunAsync(string fileName, params string[] arguments)
        {
            return Task.Run(() =>
            {
                using Process process = new Process();
                process.StartInfo = ProcessRunner.CreateStartInfo(fileName, arguments);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return process.ExitCode;
            });
        }

        internal static ProcessStartInfo CreateStartInfo(string fileName, params string[] arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    startInfo.ArgumentList.Add(argument);
                }
            }

            return startInfo;
        }
    }
}