using GitIssue.Fields;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Keys;

namespace GitIssue.Editors
{
    public class Editor : IEditor
    {
        private static char CommentChar = '#';

        private static char Newline = '\n';

        private static string FieldTemplate = 
            $"{Newline}{CommentChar} Please edit the field with your updates. Lines starting" +
            $"{Newline}{CommentChar} with '#' will be ignored, leave the file unchanged to abort. ";

        private static string FieldHeaderRegex = @$"^{CommentChar}[\s]?([\w]*)[\s]?$";

        /// <summary>
        /// Gets or sets the successful result
        /// </summary>
        public int Success { get; set; } = 0;

        /// <summary>
        /// Gets or sets the command
        /// </summary>
        public string Command { get; set; } = "joe";

        /// <summary>
        /// Gets or sets the arguments
        /// </summary>
        public string Arguments { get; set; } = "-pound_comment -syntax git-commit";

        /// <inheritdoc />
        public async Task Open(IIssue issue)
        {
            // Convert fields to a file
            string temp = GetTempFile();
            foreach (var field in issue.Values)
            {
                await File.AppendAllTextAsync(temp, $"{CommentChar} {field.Key} {Newline}");
                await File.AppendAllTextAsync(temp, $"{await field.ExportAsync()}{Newline}");
            }
            await File.AppendAllTextAsync(temp, FieldTemplate);

            // Open and modify the file
            DateTime created = File.GetLastWriteTime(temp);
            if (await EditFileAsync(this.Command, this.Arguments + " " + temp))
            {
                if (created == File.GetLastWriteTime(temp))
                    return;
            }
            else
            {
                return;
            }

            // Extract the field updates
            FieldKey key = FieldKey.None;
            string content = string.Empty;
            Dictionary<FieldKey, string> updates = new Dictionary<FieldKey, string>();
            await foreach(var line in ReadLinesAsync(temp))
            {
                if (IsMatch(line, FieldHeaderRegex, out Match match))
                {
                    if (key != FieldKey.None)
                    {
                        updates[key] = content;
                        key = FieldKey.None;
                        content = string.Empty;
                    }
                    key = FieldKey.Create(match.Groups[1].Value.Trim());
                }
                else if (key != FieldKey.None)
                {
                    if (string.IsNullOrEmpty(content))
                    {
                        content = line;
                    }
                    else
                    {
                        content = content + Newline + line;
                    }
                }
            }

            // Update the fields
            foreach (var field in issue.Values)
            {
                if (updates.ContainsKey(field.Key))
                {
                    await field.UpdateAsync(updates[field.Key]);
                }
            }
        }

        /// <inheritdoc />
        public async Task Open(IField field)
        {
            string content = await field.ExportAsync();
            string temp = GetTempFile();
            await File.WriteAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, FieldTemplate);

            DateTime created = File.GetLastWriteTime(temp);
            if (await EditFileAsync(this.Command, this.Arguments + " " + temp))
            {
                if (created != File.GetLastWriteTime(temp))
                    return;
            }

            await field.UpdateAsync(RemoveComments(await File.ReadAllTextAsync(temp)));
        }

        private static string GetTempFile() => Path.GetTempFileName();

        private static bool IsMatch(string input, string pattern, out Match match)
        {
            if (!string.IsNullOrEmpty(input))
            {
                match = Regex.Match(input, pattern);
                return match.Success;
            }
            match = null;
            return false;
        }
        private static async IAsyncEnumerable<string> ReadLinesAsync(string file)
        {
            await using Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(stream);
            string line = await reader.ReadLineAsync();
            while (line != null)
            {
                yield return line;
                line = await reader.ReadLineAsync();
            }
        }

        private static string RemoveComments(string input)
        {
            string comments = $@"^{CommentChar}(.*)$";
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(l => Regex.IsMatch(l, comments) == false)
                .ToArray();
            return string.Join(Newline, lines);
        }

        private async Task<bool> EditFileAsync(string editor, string arguments)
        {
            int result = 0;
            await Task.Run(() =>
            {
                using Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = editor,
                        Arguments =  arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                result = process.ExitCode;
            });
            return result == this.Success;
        }
    }
}
