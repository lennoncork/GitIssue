﻿using System;
using System.Collections.Generic;
using System.IO;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.File;
using GitIssue.Issues.Json;
using GitIssue.Values;
using Newtonsoft.Json;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue
{
    /// <summary>
    ///     Class defining the configuration of all issues in a repository
    /// </summary>
    [JsonObject]
    public class IssueConfiguration : IIssueConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueConfiguration" /> class
        /// </summary>
        public IssueConfiguration()
        {
            KeyProvider = TypeValue.Create<FileIssueKeyProvider>();
            Fields = new Dictionary<FieldKey, FieldInfo>
            {
                {FieldKey.Create("Key"), new FieldInfo<IssueKey, JsonValueField>(true)},
                {FieldKey.Create("Title"), new FieldInfo<String, JsonValueField>(true)},
                {FieldKey.Create("Description"), new FieldInfo<String, JsonValueField>(true)},
                {FieldKey.Create("Author"), new FieldInfo<Signature, JsonValueField>(true)},
                {FieldKey.Create("Created"), new FieldInfo<DateTime, JsonValueField>(true)},
                {FieldKey.Create("Updated"), new FieldInfo<DateTime, JsonValueField>(true)},
                {FieldKey.Create("Labels"), new FieldInfo<Label, JsonArrayField>(false)},
                {FieldKey.Create("Comments"), new FieldInfo<String, JsonArrayField>(false)}
            };
        }

        /// <inheritdoc/>
        [JsonProperty]
        public TypeValue KeyProvider { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public Dictionary<FieldKey, FieldInfo> Fields { get; set; }

        /// <inheritdoc/>
        public void Save(string file)
        {
            Save<IssueConfiguration>(file);
        }

        /// <inheritdoc/>
        public void Save<T>(string file) where T : IssueConfiguration, new()
        {
            try
            {
                using var stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                using var writer = new StreamWriter(stream);
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Ignore
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
        public static T Read<T>(string file) where T : IssueConfiguration, new()
        {
            try
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var serializer = new JsonSerializer();
                var configuration = (T)serializer.Deserialize(reader, typeof(T))!;
                return configuration;
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as config file ", ex);
            }
        }
    }
}