using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.File;
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
            this.KeyProvider = TypeValue.Create<FileIssueKeyProvider>();
            this.Fields = new Dictionary<FieldKey, FieldInfo>
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

        /// <inheritdoc />
        public Dictionary<FieldKey, FieldInfo> Fields { get; set; }

        /// <inheritdoc />
        public TypeValue KeyProvider { get; set; }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
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
        public static T Read<T>(string file) where T : IssueConfiguration, new()
        {
            try
            {
                string json = File.ReadAllText(file);
                return JsonSerializer.Deserialize<T>(json, JsonSerialization.Options)
                    ?? throw new JsonException($"Unable to deserialize {file} as config file");
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as config file ", ex);
            }
        }

        /// <inheritdoc />
        public void Save(string file)
        {
            this.Save<IssueConfiguration>(file);
        }

        /// <inheritdoc />
        public void Save<T>(string file) where T : IssueConfiguration, new()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, typeof(T), JsonSerialization.IndentedOptions);
                Paths.WriteAllTextAsync(file, json).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to serialize {file} as config file ", ex);
            }
        }
    }
}