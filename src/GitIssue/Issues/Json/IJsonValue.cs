using GitIssue.Values;
using System.Text.Json.Nodes;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Interface for fields that can be converted to JSON
    /// </summary>
    public interface IJsonValue : IValue
    {
        /// <summary>
        ///     Converts the field to a JSON token for serialization
        /// </summary>
        /// <returns></returns>
        JsonNode? ToJson();
    }
}