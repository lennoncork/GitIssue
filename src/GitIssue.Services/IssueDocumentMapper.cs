using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Exceptionable;
using GitIssue.Issues;
using GitIssue.Issues.Json;

namespace GitIssue.Services
{
    /// <summary>
    ///     Maps issue documents to the JSON shape already stored by GitIssue
    /// </summary>
    public static class IssueDocumentMapper
    {
        /// <summary>
        ///     Gets the serializer options used for client payloads
        /// </summary>
        public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        /// <summary>
        ///     Creates a document from an issue
        /// </summary>
        /// <param name="issue">the issue to map</param>
        /// <returns>the mapped document</returns>
        [Throws<IssueOperationException>]
        public static IssueDocument FromIssue(IIssue issue)
        {
            if (issue is not IJsonIssue jsonIssue)
            {
                throw new IssueOperationException($"Issue \"{issue.Key}\" cannot be read as JSON.");
            }

            JsonObject json = jsonIssue.ToJson();
            Dictionary<string, JsonElement> fields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, JsonNode?> property in json)
            {
                fields[property.Key] = IssueDocumentMapper.toElement(property.Value);
            }

            return new IssueDocument
            {
                Key = issue.Key.ToString(),
                IssueType = issue.IssueType,
                Title = IssueDocumentMapper.readString(json, "Title"),
                Description = IssueDocumentMapper.readString(json, "Description"),
                Author = IssueDocumentMapper.readString(json, "Author"),
                Created = IssueDocumentMapper.readString(json, "Created"),
                Updated = IssueDocumentMapper.readString(json, "Updated"),
                Comments = IssueDocumentMapper.readComments(json),
                Fields = fields,
            };
        }

        /// <summary>
        ///     Creates a summary from a document
        /// </summary>
        /// <param name="document">the document to summarize</param>
        /// <returns>the summary</returns>
        public static IssueSummary ToSummary(IssueDocument document)
        {
            return new IssueSummary
            {
                Key = document.Key,
                Title = document.Title,
                Description = document.Description,
            };
        }

        /// <summary>
        ///     Serializes a value with <see cref="JsonOptions" />
        /// </summary>
        /// <param name="value">the value to serialize</param>
        /// <returns>the JSON text</returns>
        public static string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, IssueDocumentMapper.JsonOptions);
        }

        private static IReadOnlyList<string> readComments(JsonObject json)
        {
            if (!IssueDocumentMapper.tryGet(json, "Comments", out JsonNode? node) || (node is not JsonArray array))
            {
                return new List<string>();
            }

            List<string> comments = new List<string>();
            foreach (JsonNode? item in array)
            {
                string text = IssueDocumentMapper.readText(item);
                if (!string.IsNullOrEmpty(text))
                {
                    comments.Add(text);
                }
            }

            return comments;
        }

        private static string readString(JsonObject json, string name)
        {
            if (!IssueDocumentMapper.tryGet(json, name, out JsonNode? node))
            {
                return string.Empty;
            }

            return IssueDocumentMapper.readText(node);
        }

        private static string readText(JsonNode? node)
        {
            if (node is null)
            {
                return string.Empty;
            }

            if ((node is JsonValue value) && value.TryGetValue(out string? text) && (text is not null))
            {
                return text;
            }

            if (node is JsonValue)
            {
                string raw = node.ToJsonString();
                if ((raw.Length >= 2) && (raw[0] == '"') && (raw[^1] == '"'))
                {
                    return JsonSerializer.Deserialize<string>(raw) ?? string.Empty;
                }

                return raw;
            }

            return node.ToJsonString();
        }

        private static JsonElement toElement(JsonNode? node)
        {
            if (node is null)
            {
                return JsonSerializer.Deserialize<JsonElement>("null");
            }

            return JsonSerializer.Deserialize<JsonElement>(node.ToJsonString());
        }

        private static bool tryGet(JsonObject json, string name, out JsonNode? node)
        {
            foreach (KeyValuePair<string, JsonNode?> property in json)
            {
                if (string.Equals(property.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    node = property.Value;
                    return true;
                }
            }

            node = null;
            return false;
        }
    }
}
