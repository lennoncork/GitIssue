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
        ///     Saves the issue as Json
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<JsonObject> ReadJsonFieldsAsync(string path)
        {
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