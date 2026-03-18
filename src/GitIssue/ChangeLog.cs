using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using GitIssue.Issues;
using Newtonsoft.Json;

namespace GitIssue
{
    /// <summary>
    ///     Records the changes made by the issue manager
    /// </summary>
    [JsonObject]
    public class ChangeLog : IChangeLog
    {
        private bool hasChanged;

        /// <inheritdoc />
        public Dictionary<IssueKey, List<string>> Log { get; set; } = new Dictionary<IssueKey, List<string>>();

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
        public static ChangeLog Read(string file)
        {
            if (!File.Exists(file))
            {
                return new ChangeLog();
            }

            try
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(stream);
                JsonSerializer serializer = new JsonSerializer();
                ChangeLog? configuration = serializer.Deserialize(reader, typeof(ChangeLog)) as ChangeLog;
                return configuration ?? new ChangeLog();
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as change log ", ex);
            }
        }

        /// <inheritdoc />
        public void Add(IssueKey key, ChangeType change)
        {
            this.Add(key, change, string.Empty);
        }

        /// <inheritdoc />
        public void Add(IssueKey key, ChangeType change, string summary)
        {
            this.hasChanged = true;
            if (!this.Log.ContainsKey(key))
            {
                this.Log[key] = new List<string>();
            }

            this.Log[key].Add($"{DateTime.Now}: {ChangeLog.GetChangeDescription(change)}");
        }

        /// <inheritdoc />
        public void Add(IIssue issue, ChangeType change)
        {
            this.Add(issue.Key, change, string.Empty);
        }

        /// <inheritdoc />
        public void Add(IIssue issue, ChangeType change, string summary)
        {
            this.Add(issue.Key, change, string.Empty);
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.hasChanged = true;
            this.Log.Clear();
        }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        public void Save(string file)
        {
            if (!this.hasChanged)
            {
                return;
            }

            try
            {
                using FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                using StreamWriter writer = new StreamWriter(stream);
                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });
                serializer.Serialize(writer, this, typeof(ChangeLog));
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to serialize {file} as change log ", ex);
            }
        }

        private static string GetChangeDescription(ChangeType change)
        {
            DescriptionAttribute? attribute = typeof(ChangeType)
                .GetField(change.ToString())
                ?.GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : change.ToString();
        }
    }
}