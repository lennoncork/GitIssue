using System;
using System.IO;
using GitIssue.Issues;
using Newtonsoft.Json;
using Serilog;

namespace GitIssue.Util
{
    /// <summary>
    ///     Tracked issue metadata
    /// </summary>
    [JsonObject]
    public class TrackedIssue
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackedIssue" /> class
        /// </summary>
        public TrackedIssue()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackedIssue" /> class
        /// </summary>
        /// <param name="key">the issue key to track</param>
        public TrackedIssue(IssueKey key)
        {
            Key = key;
            Started = DateTime.Now;
        }

        /// <summary>
        ///     Gets the key of the tracked issue
        /// </summary>
        [JsonProperty]
        public IssueKey Key { get; set; } = IssueKey.None;

        /// <summary>
        ///     Gets the started date of the tracking
        /// </summary>
        [JsonProperty]
        public DateTime Started { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     Gets the none tracked issue
        /// </summary>
        public static TrackedIssue None => new TrackedIssue();

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <param name="logger">the logger</param>
        public void Save(string file, ILogger logger = null)
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
                serializer.Serialize(writer, this, typeof(TrackedIssue));
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to serialize {file} as tracked issue file ", ex);
            }
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the tracking file</param>
        /// <param name="logger">the logger</param>
        /// <returns>the <see cref="TrackedIssue" /></returns>
        public static TrackedIssue Read(string file, ILogger logger = null)
        {
            try
            {
                if (File.Exists(file))
                {
                    using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(stream);
                    var serializer = new JsonSerializer();
                    var tracked = (TrackedIssue) serializer.Deserialize(reader, typeof(TrackedIssue));
                    return tracked;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to deserialize {file} as tracked issue file ", ex);
            }

            return None;
        }
    }
}