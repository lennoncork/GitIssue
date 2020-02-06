﻿using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Json
{
    /// <summary>
    ///     Extension methods for converting a file to json
    /// </summary>
    public static class JsonIssueExtensions
    {
        /// <summary>
        ///     Saves the issue as Json
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task SaveAsJsonAsync(this IIssue issue, string path)
        {
            // Convert the issue to json
            var json = new JObject();
            foreach (var field in issue.Values)
                if (field is IJsonField jsonField)
                    json[field.Key.ToString()] = jsonField.ToJson();

            // Save the json
            await using var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            using JsonWriter writer = new JsonTextWriter(new StreamWriter(stream));
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, json);
        }

        /// <summary>
        ///     Saves the issue as Json
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<JObject> ReadJsonFieldsAsync(string path)
        {
            // Read the json
            JObject json;
            await using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
            using JsonReader reader = new JsonTextReader(new StreamReader(stream));
            var serializer = new JsonSerializer();
            json = (JObject) serializer.Deserialize(reader);
            return json;
        }
    }
}