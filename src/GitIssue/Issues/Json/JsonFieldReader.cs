using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Field reader for Json
    /// </summary>
    public class JsonFieldReader : FieldReader
    {
        /// <inheritdoc />
        public override bool CanCreateField(FieldInfo info)
        {
            if (JsonFieldReader.IsValueField(info) || JsonFieldReader.IsArrayField(info))
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool CanReadField(FieldInfo info)
        {
            if (JsonFieldReader.IsValueField(info) || JsonFieldReader.IsArrayField(info))
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override IField CreateField<T>(Issue issue, FieldKey key, FieldInfo info)
        {
            if (JsonFieldReader.IsValueField(info))
            {
                return new JsonValueField<T>(key);
            }

            if (JsonFieldReader.IsArrayField(info))
            {
                return new JsonArrayField<T>(key, new T[0]);
            }

            return null!;
        }

        /// <inheritdoc />
        public override async Task<IField> ReadFieldAsync<T>(Issue issue, FieldKey key, FieldInfo info)
        {
            if (issue is IJsonIssue jsonIssue)
            {
                JsonObject json = await JsonIssueExtensions.ReadJsonFieldsAsync(jsonIssue.Json);
                if (json.TryGetPropertyValue(key.ToString(), out JsonNode? token))
                {
                    if (JsonFieldReader.IsValueField(info) && token is JsonValue jsonValue)
                    {
                        if (this.TryGetValue(jsonValue, info, out T value))
                        {
                            return new JsonValueField<T>(key, value);
                        }
                    }

                    if (JsonFieldReader.IsArrayField(info) && token is JsonArray jsonArray)
                    {
                        T[] values = jsonArray
                            .Select(node => node as JsonValue)
                            .Where(t => t != null)
                            .Select(t => (s: this.TryGetValue(t!, info, out T v), r: v))
                            .Where(t => t.s)
                            .Select(t => t.r)
                            .ToArray();

                        return new JsonArrayField<T>(key, values);
                    }
                }

                return this.CreateField<T>(issue, key, info);
            }

            return null!;
        }

        private static bool IsArrayField(FieldInfo info)
        {
            return info.FieldType.Type == typeof(JsonArrayField);
        }

        private static bool IsValueField(FieldInfo info)
        {
            return info.FieldType.Type == typeof(JsonValueField);
        }

        private bool TryGetValue<T>(JsonValue jsonValue, FieldInfo info, out T value)
        {
            if (!JsonSerialization.TryGetScalar(jsonValue, out object? scalar))
            {
                value = default!;
                return false;
            }

            if (string.IsNullOrEmpty(info.ValueMetadata))
            {
                return ValueExtensions.TryParse(scalar, out value);
            }

            return ValueExtensions.TryParse(scalar, out value, info.ValueMetadata);
        }
    }
}