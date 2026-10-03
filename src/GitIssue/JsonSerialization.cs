using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Values;

namespace GitIssue
{
    internal static class JsonSerialization
    {
        public static JsonSerializerOptions Options { get; } = CreateOptions(false);

        public static JsonSerializerOptions IndentedOptions { get; } = CreateOptions(true);

        public static JsonNode? ToNode(object? value)
        {
            if (value is IJsonValue jsonValue)
            {
                return jsonValue.ToJson();
            }

            return value == null
                ? null
                : JsonSerializer.SerializeToNode(value, value.GetType(), Options);
        }

        public static bool TryGetScalar(JsonValue value, out object? result)
        {
            if (value.TryGetValue(out string? stringValue))
            {
                result = stringValue;
                return true;
            }

            if (value.TryGetValue(out long longValue))
            {
                result = longValue;
                return true;
            }

            if (value.TryGetValue(out decimal decimalValue))
            {
                result = decimalValue;
                return true;
            }

            if (value.TryGetValue(out double doubleValue))
            {
                result = doubleValue;
                return true;
            }

            if (value.TryGetValue(out bool booleanValue))
            {
                result = booleanValue;
                return true;
            }

            result = null;
            return false;
        }

        private static JsonSerializerOptions CreateOptions(bool writeIndented)
        {
            DefaultJsonTypeInfoResolver resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(typeInfo =>
            {
                if (!typeof(FieldInfo).IsAssignableFrom(typeInfo.Type))
                {
                    return;
                }

                foreach (JsonPropertyInfo property in typeInfo.Properties)
                {
                    if (property.Name == nameof(FieldInfo.FieldMetadata) ||
                        property.Name == nameof(FieldInfo.ValueMetadata))
                    {
                        property.ShouldSerialize = (_, value) => value is string text && !string.IsNullOrEmpty(text);
                    }
                }
            });

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = resolver,
                WriteIndented = writeIndented,
            };
            options.Converters.Add(new FieldKeyJsonConverter());
            options.Converters.Add(new IssueKeyJsonConverter());
            options.Converters.Add(new TypeValueJsonConverter());
            return options;
        }

        private sealed class FieldKeyJsonConverter : JsonConverter<FieldKey>
        {
            public override FieldKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return FieldKey.Create(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, FieldKey value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }

            public override FieldKey ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return FieldKey.Create(reader.GetString());
            }

            public override void WriteAsPropertyName(Utf8JsonWriter writer, FieldKey value, JsonSerializerOptions options)
            {
                writer.WritePropertyName(value.ToString());
            }
        }

        private sealed class IssueKeyJsonConverter : JsonConverter<IssueKey>
        {
            public override IssueKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return IssueKey.Create(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, IssueKey value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }

            public override IssueKey ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return IssueKey.Create(reader.GetString());
            }

            public override void WriteAsPropertyName(Utf8JsonWriter writer, IssueKey value, JsonSerializerOptions options)
            {
                writer.WritePropertyName(value.ToString());
            }
        }

        private sealed class TypeValueJsonConverter : JsonConverter<TypeValue>
        {
            public override TypeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? value = reader.GetString();
                if (value == null || !TypeValue.TryParse(value, out TypeValue result) || result.Type == null)
                {
                    throw new JsonException($"Unable to resolve type '{value}'.");
                }

                return result;
            }

            public override void Write(Utf8JsonWriter writer, TypeValue value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}