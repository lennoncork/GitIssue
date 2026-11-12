using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GitIssue.Issues.Json;
using GitIssue.Issues.Markdown;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Reads the issue type before selecting a field schema.
    /// </summary>
    public static class IssueTypeReader
    {
        /// <summary>
        ///     Reads an issue type, using the configured default for legacy documents.
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="configuration">the repository configuration</param>
        /// <returns>the configured issue type</returns>
        public static async Task<string> ReadAsync(IssueRoot root, IIssueConfiguration configuration)
        {
            string path = configuration.Storage == IssueStorage.Markdown
                ? MarkdownIssue.GetMarkdownFile(root)
                : JsonIssue.GetJsonFile(root);
            string text = await System.IO.File.ReadAllTextAsync(path);
            string? type = configuration.Storage == IssueStorage.Markdown
                ? MarkdownIssueFormat.ReadIssueType(text)
                : IssueTypeReader.readJson(text);
            string selected = type ?? configuration.IssueType;
            configuration.GetFields(selected);
            return selected;
        }

        private static string? readJson(string text)
        {
            using JsonDocument document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("Issue document must be a JSON object.");
            }

            string? type = null;
            bool found = false;
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (property.Name != "IssueType")
                {
                    continue;
                }

                if (found || property.Value.ValueKind != JsonValueKind.String)
                {
                    throw new InvalidDataException("IssueType must appear once as a string.");
                }

                found = true;
                type = property.Value.GetString();
            }

            if (!found)
            {
                return null;
            }

            try
            {
                return IssueTypeName.Validate(type);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("Invalid IssueType in JSON issue.", exception);
            }
        }
    }
}
