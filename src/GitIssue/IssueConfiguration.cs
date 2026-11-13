using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Exceptionable;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Values;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue
{
    /// <summary>
    ///     Class defining the configuration of all issues in a repository
    /// </summary>
    public class IssueConfiguration : IIssueConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueConfiguration" /> class
        /// </summary>
        public IssueConfiguration()
        {
            this.KeyProvider = TypeValue.Create<DateIssueKeyProvider>();
            this.IssueType = "Issue";
            this.KeyTemplate = IssueKeyTemplate.Date;
            this.Storage = IssueStorage.Markdown;
            this.Issues = new List<IssueTypeConfiguration>
            {
                new IssueTypeConfiguration
                {
                    Name = this.IssueType,
                    Fields = IssueConfiguration.CreateDefaultFields(),
                },
            };
        }

        /// <inheritdoc />
        public List<IssueTypeConfiguration> Issues { get; set; } = new List<IssueTypeConfiguration>();

        /// <inheritdoc />
        public TypeValue KeyProvider { get; set; }

        /// <inheritdoc />
        public string IssueType { get; set; }

        /// <inheritdoc />
        public string KeyTemplate { get; set; }

        /// <inheritdoc />
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public IssueStorage Storage { get; set; } = IssueStorage.Markdown;

        /// <summary>
        ///     Creates the default Markdown field schema.
        /// </summary>
        /// <returns>the default fields</returns>
        public static Dictionary<FieldKey, FieldInfo> CreateDefaultFields()
        {
            return new Dictionary<FieldKey, FieldInfo>
            {
                { FieldKey.Create("Key"), new FieldInfo<IssueKey, JsonValueField>(true) },
                { FieldKey.Create("Title"), new FieldInfo<String, JsonValueField>(true) },
                { FieldKey.Create("Description"), new FieldInfo<String, JsonValueField>(true) },
                { FieldKey.Create("Author"), new FieldInfo<Signature, JsonValueField>(true) },
                { FieldKey.Create("Created"), new FieldInfo<DateTime, JsonValueField>(true) },
                { FieldKey.Create("Updated"), new FieldInfo<DateTime, JsonValueField>(true) },
                { FieldKey.Create("Labels"), new FieldInfo<Label, JsonArrayField>(false) },
                { FieldKey.Create("Comments"), new FieldInfo<String, JsonArrayField>(false) },
            };
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
        [Throws<AggregateException>]
        public static IssueConfiguration Read(string file)
        {
            return IssueConfiguration.Read<IssueConfiguration>(file);
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
        [Throws<AggregateException>]
        public static T Read<T>(string file) where T : IssueConfiguration, new()
        {
            try
            {
                string json = File.ReadAllText(file);
                T configuration = JsonSerializer.Deserialize<T>(json, JsonSerialization.Options) ?? throw new JsonException($"Unable to deserialize {file} as config file");
                configuration.validateTypes();
                return configuration;
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as config file ", ex);
            }
        }

        /// <summary>
        ///     Gets the stored field schema for an issue type.
        /// </summary>
        /// <param name="issueType">the issue type identifier</param>
        /// <returns>the issue type's stored fields</returns>
        public Dictionary<FieldKey, FieldInfo> GetSchema(string issueType)
        {
            IssueTypeName.Validate(issueType);
            if (this.Issues == null)
            {
                throw new ArgumentException("Issues must be an array.", nameof(issueType));
            }

            IssueTypeConfiguration? match = this.Issues.SingleOrDefault(
                issue => string.Equals(issue.Name, issueType, StringComparison.Ordinal));
            if (match?.Fields == null)
            {
                throw new ArgumentException($"Unknown issue type '{issueType}'.", nameof(issueType));
            }

            return match.Fields;
        }

        /// <inheritdoc />
        public Dictionary<FieldKey, FieldInfo> GetFields(string issueType)
        {
            Dictionary<FieldKey, FieldInfo> schema = new Dictionary<FieldKey, FieldInfo>(this.GetSchema(issueType));
            if (schema.Keys.Any(key => string.Equals(key.ToString(), "IssueType", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("'IssueType' is reserved for the issue type identifier.");
            }

            schema.Add(FieldKey.Create("IssueType"), new FieldInfo<String, JsonValueField>(false));
            return schema;
        }

        /// <inheritdoc />
        [Throws<AggregateException>]
        public void Save(string file)
        {
            this.Save<IssueConfiguration>(file);
        }

        /// <inheritdoc />
        [Throws<AggregateException>]
        public void Save<T>(string file) where T : IssueConfiguration, new()
        {
            try
            {
                this.validateTypes();
                this.normalizeKeyTemplate();
                string json = JsonSerializer.Serialize(this, typeof(T), JsonSerialization.IndentedOptions);
                Paths.WriteAllTextAsync(file, json).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to serialize {file} as config file ", ex);
            }
        }

        private void normalizeKeyTemplate()
        {
            if ((this.KeyProvider.Type == typeof(NameIssueKeyProvider)) &&
                (string.IsNullOrWhiteSpace(this.KeyTemplate) || (this.KeyTemplate == IssueKeyTemplate.Date)))
            {
                this.KeyTemplate = IssueKeyTemplate.Name;
            }
        }

        private void validateTypes()
        {
            IssueTypeName.Validate(this.IssueType);
            if (this.Issues == null)
            {
                throw new ArgumentException("Issues must be an array.");
            }

            if (this.Issues.Count == 0)
            {
                throw new ArgumentException("At least one issue type is required.");
            }

            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            foreach (IssueTypeConfiguration issue in this.Issues)
            {
                if (issue == null)
                {
                    throw new ArgumentException("Issue type definitions cannot be null.");
                }

                IssueTypeName.Validate(issue.Name);
                if (!names.Add(issue.Name))
                {
                    throw new ArgumentException($"Duplicate issue type '{issue.Name}'.");
                }

                if (issue.Fields == null)
                {
                    throw new ArgumentException($"Issue type '{issue.Name}' has no field schema.");
                }

                if (issue.Fields.Keys.Any(key => string.Equals(key.ToString(), "IssueType", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("'IssueType' is reserved for the issue type identifier.");
                }

                if (issue.Fields.Any(field => field.Value == null))
                {
                    throw new ArgumentException($"Issue type '{issue.Name}' contains an empty field definition.");
                }

                Dictionary<FieldKey, FieldInfo> core = IssueConfiguration.CreateDefaultFields();
                foreach (string fieldName in new[] { "Key", "Title", "Description", "Author", "Created", "Updated" })
                {
                    FieldKey fieldKey = FieldKey.Create(fieldName);
                    if (!issue.Fields.TryGetValue(fieldKey, out FieldInfo? actual) || actual == null ||
                        actual.ValueType.Type != core[fieldKey].ValueType.Type ||
                        actual.FieldType.Type != core[fieldKey].FieldType.Type ||
                        !actual.Required)
                    {
                        throw new ArgumentException($"Issue type '{issue.Name}' requires the standard '{fieldName}' field.");
                    }
                }
            }

            if (!names.Contains(this.IssueType))
            {
                throw new ArgumentException($"Default issue type '{this.IssueType}' is not configured.");
            }
        }
    }
}
