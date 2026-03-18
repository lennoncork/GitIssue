using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Tool
{
    /// <summary>
    ///     Editor class
    /// </summary>
    public class Editor : IEditor
    {
        private static readonly char CommentChar = '#';

        private static readonly string FieldHeaderRegex = @$"^{Editor.CommentChar}[\s]?([\w]*)[\s]?$";

        private static readonly char Newline = '\n';

        private static readonly string FieldTemplate =
            $"{Editor.Newline}{Editor.CommentChar} Please edit the field with your updates. Lines starting" +
            $"{Editor.Newline}{Editor.CommentChar} with '#' will be ignored, leave the file unchanged to abort. ";



        public Editor(Configuration configuration)
        {
            this.Command = configuration.Editor;
            this.Arguments = configuration.Arguments;
        }

        /// <summary>
        ///     Gets or sets the command
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the command
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the successful result
        /// </summary>
        public int Success { get; set; } = 0;

        public void UpdateCommand(string command)
        {
            (string, string) result = Editor.GetProcessAndArgumentsFromCommand(command);
            this.Command = result.Item1;
            this.Arguments = result.Item2;
        }

        /// <inheritdoc />
        public async Task<string> Edit(string header, string content)
        {
            string temp = Editor.GetTempFile();

            await File.AppendAllTextAsync(temp, $"{Editor.CommentChar} {header} {Editor.Newline}");
            await File.AppendAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, Editor.FieldTemplate);

            DateTime created = File.GetLastWriteTime(temp);

            if (await this.EditFileAsync(this.Command, this.Arguments + " " + temp))
            {
                if (created == File.GetLastWriteTime(temp))
                {
                    return content;
                }
            }

            return Editor.RemoveComments(await File.ReadAllTextAsync(temp));
        }

        /// <inheritdoc />
        public async Task Open(IIssue issue)
        {
            await this.Open(issue.Values);
        }

        /// <inheritdoc />
        public async Task Open(IEnumerable<IField> fields)
        {
            // Convert fields to a file
            string temp = Editor.GetTempFile();
            foreach (IField field in fields)
            {
                await File.AppendAllTextAsync(temp, $"{Editor.CommentChar} {field.Key} {Editor.Newline}");
                await File.AppendAllTextAsync(temp, $"{await field.ExportAsync()}{Editor.Newline}");
            }

            await File.AppendAllTextAsync(temp, Editor.FieldTemplate);

            // Open and modify the file
            DateTime created = File.GetLastWriteTime(temp);
            if (await this.EditFileAsync(this.Command, this.Arguments + " " + temp))
            {
                if (created == File.GetLastWriteTime(temp))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            // Extract the field updates
            FieldKey key = FieldKey.None;
            string content = string.Empty;
            Dictionary<FieldKey, string> updates = new Dictionary<FieldKey, string>();
            await foreach (string line in Editor.ReadLinesAsync(temp))
            {
                if (Editor.IsMatch(line, Editor.FieldHeaderRegex, out Match match))
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
                        content = content + Editor.Newline + line;
                    }
                }
            }

            // Update the fields
            foreach (IField field in fields)
            {
                if (updates.ContainsKey(field.Key))
                {
                    field.Update(updates[field.Key]);
                }
            }
        }

        /// <inheritdoc />
        public async Task Open(IField field)
        {
            string content = await field.ExportAsync();
            string temp = Editor.GetTempFile();
            await File.WriteAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, Editor.FieldTemplate);

            DateTime created = File.GetLastWriteTime(temp);
            if (await this.EditFileAsync(this.Command, this.Arguments + " " + temp))
            {
                if (created == File.GetLastWriteTime(temp))
                {
                    return;
                }
            }

            field.Update(Editor.RemoveComments(await File.ReadAllTextAsync(temp)));
        }

        private static (string, string) GetProcessAndArgumentsFromCommand(string command)
        {
            try
            {
                Match match = Regex.Match(command, "^\\s?([\\w.]+|\"[\\w\\s.]*\")\\s?(.*)?$");
                if (match.Success)
                {
                    if (match.Groups.Count == 3)
                    {
                        return (match.Groups[1].ToString().Trim(), match.Groups[2].ToString().Trim());
                    }
                }
            }
            catch (Exception)
            {
                // Ignored
            }

            return (command.Trim(), string.Empty);
        }

        private static string GetTempFile()
        {
            return Path.GetTempFileName();
        }

        private static bool IsMatch(string input, string pattern, out Match match)
        {
            if (!string.IsNullOrEmpty(input))
            {
                match = Regex.Match(input, pattern);
                return match.Success;
            }

            match = null!;
            return false;
        }

        private static async IAsyncEnumerable<string> ReadLinesAsync(string file)
        {
            await using Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(stream);
            string? line = await reader.ReadLineAsync();
            while (line != null)
            {
                yield return line;
                line = await reader.ReadLineAsync();
            }
        }

        private static string RemoveComments(string input)
        {
            string comments = $@"^{Editor.CommentChar}(.*)$";
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(l => !Regex.IsMatch(l, comments))
                .ToArray();
            return string.Join(Editor.Newline, lines);
        }

        private async Task<bool> EditFileAsync(string editor, string arguments)
        {
            int result = 0;
            await Task.Run(() =>
            {
                using Process process = new Process();

                process.StartInfo.FileName = editor;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

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