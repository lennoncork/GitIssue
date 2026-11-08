using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Issues;
using GitIssue.Issues.File;
using GitIssue.Issues.Json;
using GitIssue.Values;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue.Validation
{
    /// <summary>
    ///     Validates issue fields against the configured schema
    /// </summary>
    public static class IssueValidator
    {
        /// <summary>
        ///     Validates the fields currently stored on an issue
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="schema">the configured fields</param>
        /// <returns>the validation errors</returns>
        public static IReadOnlyList<IssueValidationError> ValidateFields(
            IIssue issue,
            IDictionary<FieldKey, FieldInfo> schema)
        {
            List<IssueValidationError> errors = new List<IssueValidationError>();
            foreach (KeyValuePair<FieldKey, FieldInfo> field in schema)
            {
                if (!issue.TryGetValue(field.Key, out IField? value) || (value == null))
                {
                    if (field.Value.Required)
                    {
                        errors.Add(IssueValidator.missing(issue.Key, field.Key));
                    }

                    continue;
                }

                if (!IssueValidator.IsValidField(field.Value, field.Key, value))
                {
                    errors.Add(new IssueValidationError(
                        issue.Key,
                        field.Key,
                        null,
                        IssueValidator.invalidFieldMessage(field.Value, value)));
                }
            }

            if ((issue is Issue located) &&
                (located.Root.Key != IssueKey.None) &&
                (located.Root.Key != issue.Key))
            {
                errors.Add(new IssueValidationError(
                    issue.Key,
                    FieldKey.Create("Key"),
                    null,
                    $"'{issue.Key}' does not match issue key '{located.Root.Key}'."));
            }

            return errors;
        }

        /// <summary>
        ///     Validates a JSON issue document
        /// </summary>
        /// <param name="json">the JSON text</param>
        /// <param name="issueKey">the issue key taken from the file path</param>
        /// <param name="schema">the configured fields</param>
        /// <returns>the validation errors</returns>
        public static IReadOnlyList<IssueValidationError> ValidateJson(
            string json,
            IssueKey issueKey,
            IDictionary<FieldKey, FieldInfo> schema)
        {
            List<IssueValidationError> errors = new List<IssueValidationError>();
            JsonObject? document;
            try
            {
                document = JsonNode.Parse(json) as JsonObject;
            }
            catch (JsonException exception)
            {
                errors.Add(new IssueValidationError(issueKey, FieldKey.None, null, $"invalid JSON: {exception.Message}"));
                return errors;
            }

            if (document == null)
            {
                errors.Add(new IssueValidationError(issueKey, FieldKey.None, null, "issue file is not a JSON object."));
                return errors;
            }

            foreach (KeyValuePair<string, JsonNode?> property in document)
            {
                if (!schema.ContainsKey(FieldKey.Create(property.Key)))
                {
                    errors.Add(new IssueValidationError(
                        issueKey,
                        FieldKey.Create(property.Key),
                        null,
                        "unknown field."));
                }
            }

            foreach (KeyValuePair<FieldKey, FieldInfo> field in schema)
            {
                if (!document.TryGetPropertyValue(field.Key.ToString(), out JsonNode? node) || (node == null))
                {
                    if (field.Value.Required)
                    {
                        errors.Add(IssueValidator.missing(issueKey, field.Key));
                    }

                    continue;
                }

                if (IssueValidator.isArray(field.Value))
                {
                    if (node is not JsonArray array)
                    {
                        errors.Add(new IssueValidationError(issueKey, field.Key, null, "value is not an array."));
                        continue;
                    }

                    if (field.Value.Required && (array.Count == 0))
                    {
                        errors.Add(new IssueValidationError(issueKey, field.Key, null, "required field is empty."));
                        continue;
                    }

                    foreach (JsonNode? item in array)
                    {
                        if ((item == null) || !IssueValidator.tryRaw(item, out string raw))
                        {
                            errors.Add(new IssueValidationError(issueKey, field.Key, null, "array item is not a value."));
                            continue;
                        }

                        IssueValidator.addRawError(errors, issueKey, field.Key, field.Value, raw, null);
                    }
                }
                else if (!IssueValidator.tryRaw(node, out string scalar))
                {
                    errors.Add(new IssueValidationError(issueKey, field.Key, null, "value is not a scalar."));
                }
                else
                {
                    IssueValidator.addRawError(errors, issueKey, field.Key, field.Value, scalar, null);
                    if (IssueValidator.tryAccept(field.Value, field.Key, scalar, out object? parsed) &&
                        (parsed is IssueKey parsedKey) &&
                        (field.Key == FieldKey.Create("Key")) &&
                        (parsedKey != issueKey))
                    {
                        errors.Add(new IssueValidationError(
                            issueKey,
                            field.Key,
                            null,
                            $"{IssueValidator.quote(scalar)} does not match issue key '{issueKey}'."));
                    }
                }
            }

            return errors;
        }

        /// <summary>
        ///     Determines whether a populated field satisfies its schema
        /// </summary>
        /// <param name="info">the field schema</param>
        /// <param name="key">the field key</param>
        /// <param name="field">the field value</param>
        /// <returns>true when the field is valid</returns>
        public static bool IsValidField(FieldInfo info, FieldKey key, IField field)
        {
            if (field is IValueField valueField)
            {
                return IssueValidator.isAcceptable(info, key, valueField.Value, false);
            }

            if (field is IArrayField arrayField)
            {
                object[] values = arrayField.Values ?? Array.Empty<object>();
                if (info.Required && (values.Length == 0))
                {
                    return false;
                }

                foreach (object value in values)
                {
                    if (!IssueValidator.isAcceptable(info, key, value, true))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Parses a raw field value and checks it against the schema
        /// </summary>
        /// <param name="info">the field schema</param>
        /// <param name="key">the field key</param>
        /// <param name="raw">the raw text</param>
        /// <param name="value">the parsed value</param>
        /// <param name="error">the validation error message</param>
        /// <returns>true when the raw text is a valid value</returns>
        public static bool TryAccept(FieldInfo info, FieldKey key, string raw, out object? value, out string? error)
        {
            if (IssueValidator.requiresText(info, key) && string.IsNullOrWhiteSpace(raw))
            {
                value = null;
                error = "required field is empty.";
                return false;
            }

            if (!IssueValidator.allowsMultipleLines(info, key) && raw.Contains('\n'))
            {
                value = null;
                error = "value must be a single line.";
                return false;
            }

            if (!IssueValidator.tryAccept(info, key, raw, out value))
            {
                value = null;
                error = $"{IssueValidator.quote(raw)} is not a valid {info.ValueType}.";
                return false;
            }

            if ((value is IssueKey issueKey) &&
                (key == FieldKey.Create("Key")) &&
                (issueKey == IssueKey.None))
            {
                value = null;
                error = "required field is empty.";
                return false;
            }

            error = null;
            return true;
        }

        internal static bool IsArray(FieldInfo info)
        {
            return IssueValidator.isArray(info);
        }

        internal static bool IsTextArray(FieldInfo info)
        {
            return IssueValidator.isArray(info) &&
                   ((info.ValueType.Type == typeof(String)) || (info.ValueType.Type == typeof(Markdown)));
        }

        private static void addRawError(
            List<IssueValidationError> errors,
            IssueKey issueKey,
            FieldKey key,
            FieldInfo info,
            string raw,
            int? line)
        {
            if (!IssueValidator.TryAccept(info, key, raw, out object? _, out string? error))
            {
                errors.Add(new IssueValidationError(issueKey, key, line, error ?? "value is not valid."));
            }
        }

        private static bool allowsMultipleLines(FieldInfo info, FieldKey key)
        {
            if (IssueValidator.isArray(info))
            {
                return IssueValidator.IsTextArray(info);
            }

            return (key == FieldKey.Create("Description")) ||
                   (info.ValueType.Type == typeof(Markdown));
        }

        private static string invalidFieldMessage(FieldInfo info, IField field)
        {
            string current = field.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(current) || (current == "Unknown"))
            {
                return "required field is empty.";
            }

            return $"{IssueValidator.quote(current)} is not a valid {info.ValueType}.";
        }

        private static bool isAcceptable(FieldInfo info, FieldKey key, object? value, bool item)
        {
            if (value == null)
            {
                return !IssueValidator.requiresText(info, key);
            }

            if (value is String text)
            {
                if (!IssueValidator.requiresText(info, key))
                {
                    return true;
                }

                return !string.IsNullOrWhiteSpace(text.Item);
            }

            if (value is Markdown markdown)
            {
                if (!info.Required)
                {
                    return true;
                }

                return !string.IsNullOrWhiteSpace(markdown.Item);
            }

            if (value is Signature signature)
            {
                return !info.Required || signature.IsValid;
            }

            if (value is DateTime dateTime)
            {
                return !info.Required || dateTime.IsValid;
            }

            if (value is Email email)
            {
                return !info.Required || email.IsValid;
            }

            if (value is Url url)
            {
                return !info.Required || url.IsValid;
            }

            if (value is Values.Version version)
            {
                return !info.Required || version.IsValid;
            }

            if (value is Label label)
            {
                bool valid = !string.IsNullOrWhiteSpace(label.Item);
                return valid || (!item && !info.Required);
            }

            if (value is Number)
            {
                return true;
            }

            if (value is Enumerated enumerated)
            {
                bool valid = (enumerated.Values != null) &&
                             (enumerated.Values.Length > 0) &&
                             (Array.IndexOf(enumerated.Values, enumerated.Item) >= 0);
                return valid || (!item && !info.Required);
            }

            if (value is IssueKey issueKey)
            {
                return issueKey != IssueKey.None;
            }

            return true;
        }

        private static bool isArray(FieldInfo info)
        {
            Type type = info.FieldType.Type;
            return (type == typeof(JsonArrayField)) || (type == typeof(FileArrayField));
        }

        private static IssueValidationError missing(IssueKey issue, FieldKey field)
        {
            return new IssueValidationError(issue, field, null, "required field is missing.");
        }

        private static string quote(string raw)
        {
            string single = raw.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
            if (single.Length > 80)
            {
                single = single.Substring(0, 77) + "...";
            }

            return $"'{single}'";
        }

        private static bool requiresText(FieldInfo info, FieldKey key)
        {
            if (!info.Required || IssueValidator.isArray(info))
            {
                return false;
            }

            if (key == FieldKey.Create("Description"))
            {
                return false;
            }

            return (info.ValueType.Type == typeof(String)) || (info.ValueType.Type == typeof(Markdown));
        }

        private static bool tryAccept(FieldInfo info, FieldKey key, string raw, out object? value)
        {
            if (!IssueValidator.tryConvert(info, raw, out value) || (value == null))
            {
                return false;
            }

            return IssueValidator.isAcceptable(info, key, value, false);
        }

        private static bool tryConvert(FieldInfo info, string raw, out object? value)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(info.ValueType.Type);
            try
            {
                if (!string.IsNullOrEmpty(info.ValueMetadata) && converter.CanConvertFrom(typeof(ValueMetadata)))
                {
                    object? converted = converter.ConvertFrom(new ValueMetadata(raw, info.ValueMetadata));
                    if (converted != null)
                    {
                        value = converted;
                        return true;
                    }
                }

                if (converter.CanConvertFrom(typeof(string)))
                {
                    object? converted = converter.ConvertFrom(null, CultureInfo.InvariantCulture, raw);
                    if (converted != null)
                    {
                        value = converted;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                value = null;
                return false;
            }

            value = null;
            return false;
        }

        private static bool tryRaw(JsonNode node, out string raw)
        {
            if ((node is JsonValue value) &&
                JsonSerialization.TryGetScalar(value, out object? scalar) &&
                (scalar != null))
            {
                raw = Convert.ToString(scalar, CultureInfo.InvariantCulture) ?? string.Empty;
                return true;
            }

            raw = string.Empty;
            return false;
        }
    }
}
