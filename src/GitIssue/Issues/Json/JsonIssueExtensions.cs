using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Extension methods for converting a file to json
    /// </summary>
    public static class JsonIssueExtensions
    {
        /// <summary>
        ///     Gets the number of times an issue JSON file has been read.
        /// </summary>
        internal static int FileReadCount { get; private set; }

        /// <summary>
        ///     Resets <see cref="FileReadCount" />. Tests use this to confirm a load reads the file once.
        /// </summary>
        internal static void ResetFileReadCount()
        {
            JsonIssueExtensions.FileReadCount = 0;
        }

        /// <summary>
        ///     Saves the issue as Json
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<JsonObject> ReadJsonFieldsAsync(string path)
        {
            JsonIssueExtensions.FileReadCount++;
            string json = await System.IO.File.ReadAllTextAsync(path);
            return JsonNode.Parse(json) as JsonObject ?? throw new JsonException($"Unable to deserialize {path} as a JSON object.");
        }

        /// <summary>
        ///     Saves the issue as Json
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task SaveAsJsonAsync(this IJsonIssue issue, string path)
        {
            string json = issue.ToJson().ToJsonString(JsonSerialization.IndentedOptions);
            await Paths.WriteAllTextAsync(path, json);
        }
    }
}