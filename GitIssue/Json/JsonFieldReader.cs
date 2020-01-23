using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using Newtonsoft.Json.Linq;

namespace GitIssue.Json
{
    /// <summary>
    /// Field reader for Json
    /// </summary>
    public class JsonFieldReader : FieldReader
    {
        /// <inheritdoc/>
        public override bool CanCreateField(FieldInfo info)
        {
            if (IsValueField(info) || IsArrayField(info))
                return true;
            return false;
        }

        /// <inheritdoc/>
        public override bool CanReadField(FieldInfo info)
        {
            if (IsValueField(info) || IsArrayField(info))
                return true;
            return false;
        }

        /// <inheritdoc/>
        public override IField CreateField<T>(Issue issue, FieldKey key, FieldInfo info)
        {
            if (IsValueField(info))
            {
                return new JsonValueField<T>(key, default(T));
            }
            if (IsArrayField(info))
            {
                return new JsonArrayField<T>(key, new T[0]);
            }
            return null;
        }

        /// <inheritdoc/>
        public override async Task<IField> ReadFieldAsync<T>(Issue issue, FieldKey key, FieldInfo info)
        {
            if (issue is IJsonIssue jsonIssue)
            {
                JObject json = await JsonIssueExtensions.ReadJsonFieldsAsync(jsonIssue.Json);
                if (json.TryGetValue(key.ToString(), out JToken token))
                {
                    if (IsValueField(info) && token is JValue jValue)
                    {
                        if (this.TryGetValue(jValue, out T value))
                        {
                            return new JsonValueField<T>(key, value);
                        }
                    }
                    if (IsArrayField(info) && token is JArray jArray)
                    {
                        T[] values = jArray
                            .Select(t => t as JValue)
                            .Select(t => t?.Value)
                            .Where(t => t?.GetType() == typeof(T))
                            .Select(t => (T)t)
                            .ToArray();

                        return new JsonArrayField<T>(key, values);
                    }
                }
                return this.CreateField<T>(issue, key, info);
            }
            return null;
        }

        private static bool IsValueField(FieldInfo info) => info.FieldType.Type == typeof(JsonValueField);

        private static bool IsArrayField(FieldInfo info) => info.FieldType.Type == typeof(JsonArrayField);

        private bool TryGetValue<T>(JValue jValue, out T value)
        {
            if (jValue.Value is T result)
            {
                value = result;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}