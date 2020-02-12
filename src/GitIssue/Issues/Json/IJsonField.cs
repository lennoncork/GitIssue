using GitIssue.Fields;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Interface for fields that can be converted to JSON
    /// </summary>
    public interface IJsonField : IField
    {
        /// <summary>
        ///     Converts the field to a JSON token for serialization
        /// </summary>
        /// <returns></returns>
        JToken ToJson();
    }
}