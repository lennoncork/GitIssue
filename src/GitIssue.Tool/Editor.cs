using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Util
{
    /// <summary>
    ///     Editor class
    /// </summary>
    public class Editor : IEditor
    {
        private static readonly char CommentChar = '#';

        private static readonly char Newline = '\n';

        private static readonly string FieldTemplate =
            $"{Newline}{CommentChar} Please edit the field with your updates. Lines starting" +
            $"{Newline}{CommentChar} with '#' will be ignored, leave the file unchanged to abort. ";

        private static readonly string FieldHeaderRegex = @$"^{CommentChar}[\s]?([\w]*)[\s]?$";

        public Editor(Configuration configuration)
        {
            this.Command = configuration.Editor;
            this.Arguments = configuration.Arguments;
        }

        /// <summary>
        ///     Gets or sets the successful result
        /// </summary>
        public int Success { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the command
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the command
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        public void UpdateCommand(string command)
        {
            var result = GetProcessAndArgumentsFromCommand(command);
            this.Command = result.Item1;
            this.Arguments = result.Item2;
        }

        private static (string, string) GetProcessAndArgumentsFromCommand(string command)
        {
            try
            {
                var match = Regex.Match(command, "^\\s?([\\w.]+|\"[\\w\\s.]*\")\\s?(.*)?$");
                if(match.Success)
                    if(match.Groups.Count == 3)
                        return (match.Groups[1].ToString().Trim(), match.Groups[2].ToString().Trim());
            }
            catch(Exception)
            {
                // Ignored
            }
            return (command.Trim(), String.Empty);
        }

        /// <inheritdoc />
        public async Task<string> Edit(string header, string content)
        {
            var temp = GetTempFile();
            
            await File.AppendAllTextAsync(temp, $"{CommentChar} {header} {Newline}");
            await File.AppendAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, FieldTemplate);

            var created = File.GetLastWriteTime(temp);

            if (await EditFileAsync(this.Command, this.Arguments + " " + temp))
                if (created == File.GetLastWriteTime(temp))
                    return content;

            return RemoveComments(await File.ReadAllTextAsync(temp));
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
            var temp = GetTempFile();
            foreach (var field in fields)
            {
                await File.AppendAllTextAsync(temp, $"{CommentChar} {field.Key} {Newline}");
                await File.AppendAllTextAsync(temp, $"{await field.ExportAsync()}{Newline}");
            }

            await File.AppendAllTextAsync(temp, FieldTemplate);

            // Open and modify the file
            var created = File.GetLastWriteTime(temp);
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
            var key = FieldKey.None;
            var content = string.Empty;
            var updates = new Dictionary<FieldKey, string>();
            await foreach (var line in ReadLinesAsync(temp))
                if (IsMatch(line, FieldHeaderRegex, out var match))
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
                        content = line;
                    else
                        content = content + Newline + line;
                }

            // Update the fields
            foreach (var field in fields)
                if (updates.ContainsKey(field.Key))
                    field.Update(updates[field.Key]);
        }

        /// <inheritdoc />
        public async Task Open(IField field)
        {
            var content = await field.ExportAsync();
            var temp = GetTempFile();
            await File.WriteAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, FieldTemplate);

            var created = File.GetLastWriteTime(temp);
            if (await EditFileAsync(this.Command, this.Arguments + " " + temp))
                if (created == File.GetLastWriteTime(temp))
                    return;

            field.Update(RemoveComments(await File.ReadAllTextAsync(temp)));
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
            using var reader = new StreamReader(stream);
            var line = await reader.ReadLineAsync();
            while (line != null)
            {
                yield return line;
                line = await reader.ReadLineAsync();
            }
        }

        private static string RemoveComments(string input)
        {
            var comments = $@"^{CommentChar}(.*)$";
            var lines = input.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None)
                .Where(l => Regex.IsMatch(l, comments) == false)
                .ToArray();
            return string.Join(Newline, lines);
        }

        private async Task<bool> EditFileAsync(string editor, string arguments)
        {
            var result = 0;
            await Task.Run(() =>
            {
                using var process = new Process();

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
            return result == Success;
        }

    }
}