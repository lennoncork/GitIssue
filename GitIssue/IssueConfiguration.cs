using System;
using System.Collections.Generic;
using System.IO;
using GitIssue.Fields;
using Newtonsoft.Json;

namespace GitIssue
{
    /// <summary>
    /// Class defining the configuration of all issues in a repository
    /// </summary>
    [JsonObject]
    public class IssueConfiguration
    {
        /// <summary>
        /// Gets or sets the dictionary of fields
        /// </summary>
        [JsonProperty]
        public Dictionary<FieldKey, FieldInfo> Fields { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueConfiguration"/> class
        /// </summary>
        public IssueConfiguration()
        {
            this.Fields = new Dictionary<FieldKey, FieldInfo>();
            
            this.Fields.Add(FieldKey.Create("Title"), new FieldInfo<string>
            {
                Required = true,
            });

            this.Fields.Add(FieldKey.Create("Description"), new FieldInfo<string>
            {
                Required = true,
            });

            this.Fields.Add(FieldKey.Create("Created"), new FieldInfo<DateTime>
            {
                Required = true,
            });

            this.Fields.Add(FieldKey.Create("Updated"), new FieldInfo<DateTime>
            {
                Required = true,
            });

        }

        /// <summary>
        /// Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        public void Save(string file) => Save<IssueConfiguration>(file);

        /// <summary>
        /// Saves the configuration to a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        public void Save<T>(string file) where T : IssueConfiguration
        {
            try
            {
                using FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using StreamWriter writer = new StreamWriter(stream);
                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                });
                serializer.Serialize(writer, this, typeof(T));
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to serialize {file} as config file ", ex);
            }
        }

        /// <summary>
        /// Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration"/></returns>
        public static IssueConfiguration Read(string file) => Read<IssueConfiguration>(file);

        /// <summary>
        /// Reads the configuration from a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration"/></returns>
        public static IssueConfiguration Read<T>(string file) where T : IssueConfiguration
        {
            try
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(stream);
                JsonSerializer serializer = new JsonSerializer();
                T version = (T)serializer.Deserialize(reader, typeof(T));
                return version;
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as config file ", ex);
            }
        }
    }
}
