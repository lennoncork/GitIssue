using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Markdown;

namespace GitIssue.Tool
{
    /// <summary>
    ///     Editor class
    /// </summary>
    public partial class Editor : IEditor
    {
        private static readonly char commentChar = '#';

        [GeneratedRegex(@"^\s?([\w.]+|""[\w\s.]*"")\s?(.*)?$")]
        private static partial Regex ProcessCommandRegex();

        private static readonly string fieldHeaderRegex = @$"^{Editor.commentChar}[\s]?([\w]*)[\s]?$";

        private static readonly char newline = '\n';

        private static readonly string fieldTemplate =
            $"{Editor.newline}{Editor.commentChar} Please edit the field with your updates. Lines starting" +
            $"{Editor.newline}{Editor.commentChar} with '#' will be ignored, leave the file unchanged to abort. ";

        private readonly IProcessRunner processRunner;

        public Editor(Configuration configuration, IProcessRunner processRunner)
        {
            this.Command = configuration.Editor;
            this.Arguments = configuration.Arguments;
            this.processRunner = processRunner;
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

        private static (string, string) getProcessAndArgumentsFromCommand(string command)
        {
            try
            {
                Match match = Editor.ProcessCommandRegex().Match(command);
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

        private static string getTempFile()
        {
            return Path.GetTempFileName();
        }

        private static bool isMatch(string input, string pattern, out Match match)
        {
            if (!string.IsNullOrEmpty(input))
            {
                match = Regex.Match(input, pattern);
                return match.Success;
            }

            match = null!;
            return false;
        }

        private static async IAsyncEnumerable<string> readLinesAsync(string file)
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

        private static string removeComments(string input)
        {
            string comments = $@"^{Editor.commentChar}(.*)$";
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(l => !Regex.IsMatch(l, comments))
                .ToArray();
            return string.Join(Editor.newline, lines);
        }

        public void UpdateCommand(string command)
        {
            (string, string) result = Editor.getProcessAndArgumentsFromCommand(command);
            this.Command = result.Item1;
            this.Arguments = result.Item2;
        }

        /// <inheritdoc />
        public async Task<string> Edit(string header, string content)
        {
            string temp = Editor.getTempFile();

            await File.AppendAllTextAsync(temp, $"{Editor.commentChar} {header} {Editor.newline}");
            await File.AppendAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, Editor.fieldTemplate);

            DateTime created = File.GetLastWriteTime(temp);

            if (!await this.editFileAsync(this.Command, this.Arguments + " " + temp))
            {
                return content;
            }

            if (created == File.GetLastWriteTime(temp))
            {
                return content;
            }

            return Editor.removeComments(await File.ReadAllTextAsync(temp));
        }

        /// <inheritdoc />
        public async Task Open(IIssue issue)
        {
            if (issue is MarkdownIssue markdownIssue)
            {
                await this.openMarkdown(markdownIssue);
                return;
            }

            await this.Open(issue.Values);
        }

        /// <inheritdoc />
        public async Task Open(IEnumerable<IField> fields)
        {
            // Convert fields to a file
            string temp = Editor.getTempFile();
            foreach (IField field in fields)
            {
                await File.AppendAllTextAsync(temp, $"{Editor.commentChar} {field.Key} {Editor.newline}");
                await File.AppendAllTextAsync(temp, $"{await field.ExportAsync()}{Editor.newline}");
            }

            await File.AppendAllTextAsync(temp, Editor.fieldTemplate);

            // Open and modify the file
            DateTime created = File.GetLastWriteTime(temp);
            if (await this.editFileAsync(this.Command, this.Arguments + " " + temp))
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
            await foreach (string line in Editor.readLinesAsync(temp))
            {
                if (Editor.isMatch(line, Editor.fieldHeaderRegex, out Match match))
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
                        content = content + Editor.newline + line;
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
            string temp = Editor.getTempFile();
            await File.WriteAllTextAsync(temp, content);
            await File.AppendAllTextAsync(temp, Editor.fieldTemplate);

            DateTime created = File.GetLastWriteTime(temp);
            if (await this.editFileAsync(this.Command, this.Arguments + " " + temp))
            {
                if (created == File.GetLastWriteTime(temp))
                {
                    return;
                }
            }

            field.Update(Editor.removeComments(await File.ReadAllTextAsync(temp)));
        }

        private async Task openMarkdown(MarkdownIssue issue)
        {
            string original = MarkdownIssueFormat.Write(issue, issue.Schema);
            string temp = Editor.getTempFile();
            await File.WriteAllTextAsync(temp, original);
            DateTime created = File.GetLastWriteTime(temp);
            if (!await this.editFileAsync(this.Command, this.Arguments + " " + temp))
            {
                return;
            }

            if (created == File.GetLastWriteTime(temp))
            {
                return;
            }

            string updated = await File.ReadAllTextAsync(temp);
            MarkdownRead read = MarkdownIssueFormat.Read(updated, issue.Key, issue.Schema);
            if (read.Errors.Count > 0)
            {
                throw new IssueValidationException(issue.Key, read.Errors);
            }

            issue.Apply(read);
        }

        private async Task<bool> editFileAsync(string editor, string arguments)
        {
            int result = await this.processRunner.RunAsync(editor, arguments);
            return result == this.Success;
        }
    }
}
