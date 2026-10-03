using System.Diagnostics;
using System.Threading.Tasks;

namespace GitIssue.Tool
{
    public class ProcessRunner : IProcessRunner
    {
        public Task<int> RunAsync(string fileName, string arguments)
        {
            return Task.Run(() =>
            {
                using Process process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return process.ExitCode;
            });
        }
    }
}