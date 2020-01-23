using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GitIssue.Fields
{
    /// <summary>
    /// Interface for fields that can be converted to JSON
    /// </summary>
    public interface IJsonField : IField
    {
        /// <summary>
        /// Converts the field to a JSON token for serialization
        /// </summary>
        /// <returns></returns>
        JToken ToJson();
    }
}
