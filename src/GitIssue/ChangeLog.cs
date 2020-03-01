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

        /// <inheritdoc />
        public void Clear()
        {
            hasChanged = true;
            Log.Clear();
        }

        /// <inheritdoc />
        public void Add(IssueKey key, ChangeType change)
        {
            Add(key, change, string.Empty);
        }

        /// <inheritdoc />
        public void Add(IssueKey key, ChangeType change, string summary)
        {
            hasChanged = true;
            if (Log.ContainsKey(key) == false)
                Log[key] = new List<string>();

            Log[key].Add($"{DateTime.Now}: {GetChangeDescription(change)}");
        }

        /// <inheritdoc />
        public void Add(IIssue issue, ChangeType change)
        {
            Add(issue.Key, change, string.Empty);
        }

        /// <inheritdoc />
        public void Add(IIssue issue, ChangeType change, string summary)
        {
            Add(issue.Key, change, string.Empty);
        }

        private static string GetChangeDescription(ChangeType change)
        {
            var attribute = typeof(ChangeType)
                .GetField(change.ToString())
                ?.GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : change.ToString();
        }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        public void Save(string file)
        {
            if (hasChanged == false)
                return;

            try
            {
                using var stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                using var writer = new StreamWriter(stream);
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                serializer.Serialize(writer, this, typeof(ChangeLog));
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to serialize {file} as change log ", ex);
            }
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="IssueConfiguration" /></returns>
        public static ChangeLog Read(string file)
        {
            if (File.Exists(file) == false)
                return new ChangeLog();

            try
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var serializer = new JsonSerializer();
                var configuration = serializer.Deserialize(reader, typeof(ChangeLog)) as ChangeLog;
                return configuration ?? new ChangeLog();
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as change log ", ex);
            }
        }
    }
}