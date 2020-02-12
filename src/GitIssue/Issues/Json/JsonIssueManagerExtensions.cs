using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Json Extensions for the Issue Manager
    /// </summary>
    public static class JsonIssueManagerExtensions
    {
        private static readonly Func<IIssue, bool> AllIssues = issue => true;

        /// <summary>
        ///     Exports all issues to a single json file
        /// </summary>
        /// <param name="manager">the issue manager</param>
        /// <param name="path">the json path</param>
        /// <returns></returns>
        public static async Task ExportAsJsonAsync(this IIssueManager manager, string path)
        {
            await ExportAsJsonAsync(manager, path, AllIssues);
        }


        /// <summary>
        ///     Exports all issues to a single json file
        /// </summary>
        /// <param name="manager">the issue manager</param>
        /// <param name="path">the json path</param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task ExportAsJsonAsync(this IIssueManager manager, string path,
            Func<IIssue, bool> predicate)
        {
            await using Stream stream = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await using var writer = new StreamWriter(stream);
            using var text = new JsonTextWriter(writer);
            {
                var json = new JObject();
                await foreach (var issue in manager.FindAsync(predicate))
                    if (issue is IJsonIssue jsonIssue)
                        json[issue.Key.ToString()] = jsonIssue.ToJson();
                text.Formatting = Formatting.Indented;
                await json.WriteToAsync(text);
            }
        }
    }
}