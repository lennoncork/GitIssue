using System;
using System.Collections.Generic;
using System.IO;
using GitIssue.Fields;
using GitIssue.Json;
using GitIssue.Keys;
using Newtonsoft.Json;

namespace GitIssue
{
    /// <summary>
    ///     Class defining the configuration of all issues in a repository
    /// </summary>
    [JsonObject]
    public class IssueConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueConfiguration" /> class
        /// </summary>
        public IssueConfiguration()
        {
            Fields = new Dictionary<FieldKey, FieldInfo>
            {
                {FieldKey.Create("Title"), new FieldInfo<string, JsonValueField>(true)},
                {FieldKey.Create("Description"), new FieldInfo<string, JsonValueField>(true)},
                {FieldKey.Create("Created"), new FieldInfo<DateTime, JsonValueField>(true)},
                {FieldKey.Create("Updated"), new FieldInfo<DateTime, JsonValueField>(true)},
                {FieldKey.Create("Labels"), new FieldInfo<Label, JsonArrayField>(false)},
                {FieldKey.Create("Comments"), new FieldInfo<string, JsonArrayField>(false)}
            };
        }

        /// <summary>
        ///     Gets or sets the dictionary of fields
        /// </summary>
        [JsonProperty]
        public Dictionary<FieldKey, FieldInfo> Fields { get; set; }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        public void Save(string file)
        {
            Save<IssueConfiguration>(file);
        }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        public void Save<T>(string file) where T : IssueConfiguration
        {
            try
            {
                using var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using var writer = new StreamWriter(stream);
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });
                serializer.Serialize(writer, this, typeof(T));
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to serialize {file} as config file ", ex);
            }
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
        public static IssueConfiguration Read(string file)
        {
            return Read<IssueConfiguration>(file);
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
        public static IssueConfiguration Read<T>(string file) where T : IssueConfiguration
        {
            try
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var serializer = new JsonSerializer();
                var version = (T) serializer.Deserialize(reader, typeof(T));
                return version;
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as config file ", ex);
            }
        }
    }
}