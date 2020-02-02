using Newtonsoft.Json.Linq;

namespace GitIssue.Json
{
    /// <summary>
    ///     Interface for issues that can be converted to JSON
    /// </summary>
    public interface IJsonIssue : IIssue
    {
        /// <summary>
        ///     Gets the Json path for the issue
        /// </summary>
        public string Json { get; }

        /// <summary>
        ///     Converts the field to a JSON object for serialization
        /// </summary>
        /// <returns></returns>
        JObject ToJson();
    }
}