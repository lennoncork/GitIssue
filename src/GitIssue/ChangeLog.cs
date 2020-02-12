using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using Newtonsoft.Json;

namespace GitIssue
{
    public enum ChangeType
    {
        [Description("Created new issue")]
        Create,
    }

    [JsonObject]
    public class ChangeLog : IChangeLog
    {
        private bool hasChanged = false;

        private Dictionary<IssueKey, IList<string>> changes = 
            new Dictionary<IssueKey, IList<string>>();

        /// <inheritdoc/>
        public IReadOnlyDictionary<IssueKey, string[]> Changes => this.changes
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());

        /// <inheritdoc/>
        public void Clear()
        {
            this.hasChanged = true;
            this.changes.Clear();
        }
        /// <inheritdoc/>
        public void Add(IIssue issue, ChangeType change) => this.Add(issue, change, String.Empty);

        /// <inheritdoc/>
        public void Add(IIssue issue, ChangeType change, string summary)
        {
            this.hasChanged = true;
            if (this.changes.ContainsKey(issue.Key) == false)
                this.changes[issue.Key] = new List<string>();

            this.changes[issue.Key].Add($"{DateTime.Now}: {GetChangeDescription(change)}");
        }

        private static string GetChangeDescription(ChangeType change)
        {
            DescriptionAttribute attribute = typeof(ChangeType)
                .GetField(change.ToString())
                .GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : change.ToString();
        }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        public void Save(string file)
        {
            if (this.hasChanged == false)
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
            if(File.Exists(file) == false)
                return new ChangeLog();

            try
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var serializer = new JsonSerializer();
                var configuration = (ChangeLog)serializer.Deserialize(reader, typeof(ChangeLog));
                return configuration;
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as change log ", ex);
            }
        }
    }
}
