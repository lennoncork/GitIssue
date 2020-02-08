using GitIssue.Fields;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GitIssue.Editors
{
    public class Editor : IEditor
    {
        public int Success { get; set; } = 0;

        public string Command { get; set; } = "joe";

        public Task Open(IIssue issue)
        {
            throw new NotImplementedException();
        }

        public async Task Open(IField field)
        {
            string content = await field.ExportAsync();
            string temp = await CopyToTempFile(content);
            DateTime created = File.GetLastWriteTime(temp);
            if (await OpenAsync(this.Command, temp))
            {
                if (created != File.GetLastWriteTime(temp))
                {
                    await field.UpdateAsync(await File.ReadAllTextAsync(temp));
                }
            }
        }

        private static async Task<string> CopyToTempFile(string input)
        {
            string temp = Path.GetTempFileName();
            await File.WriteAllTextAsync(temp, input);
            return temp;
        }

        private async Task<bool> OpenAsync(string editor, string file)
        {
            int result = 0;
            await Task.Run(() =>
            {
                using Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = editor,
                        Arguments = file,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                //process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => this.logger?.Debug(e.Data));
                //process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => this.logger?.Error(e.Data));
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                result = process.ExitCode;
            });
            return result == this.Success;
        }
    }
}
