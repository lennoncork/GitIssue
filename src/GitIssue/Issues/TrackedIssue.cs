using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Tracked issue metadata
    /// </summary>
    [JsonObject]
    public class TrackedIssue : ITrackedIssue
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
            this.Key = key;
            this.Started = DateTime.Now;
        }

        /// <summary>
        ///     Gets the none tracked issue
        /// </summary>
        public static TrackedIssue None => new TrackedIssue();

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
        ///     Equals
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(TrackedIssue lhs, TrackedIssue rhs)
        {
            if (string.IsNullOrEmpty(lhs?.Key) && string.IsNullOrEmpty(rhs?.Key))
            {
                return true;
            }

            if (string.IsNullOrEmpty(lhs?.Key) || string.IsNullOrEmpty(rhs?.Key))
            {
                return false;
            }

            return rhs.Equals(lhs);
        }

        /// <summary>
        ///     Not Equals
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(TrackedIssue lhs, TrackedIssue rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the tracking file</param>
        /// <param name="logger">the logger</param>
        /// <returns>the <see cref="TrackedIssue" /></returns>
        public static TrackedIssue Read(string file, ILogger? logger = null)
        {
            try
            {
                if (System.IO.File.Exists(file))
                {
                    using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    using StreamReader reader = new StreamReader(stream);
                    JsonSerializer serializer = new JsonSerializer();
                    TrackedIssue tracked = (TrackedIssue)serializer.Deserialize(reader, typeof(TrackedIssue))!;
                    return tracked;
                }
            }
            catch (Exception ex)
            {
                logger?.Error($"Unable to deserialize {file} as tracked issue file ", ex);
            }

            return TrackedIssue.None;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is TrackedIssue track)
            {
                if (string.IsNullOrEmpty(this.Key) &&
                    string.IsNullOrEmpty(track.Key))
                {
                    return true;
                }

                return this.Key == track.Key;
            }

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <param name="logger">the logger</param>
        public async Task SaveAsync(string file, ILogger? logger = null)
        {
            try
            {
                await using FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                await using StreamWriter writer = new StreamWriter(stream);
                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });
                serializer.Serialize(writer, this, typeof(TrackedIssue));
            }
            catch (Exception ex)
            {
                logger?.Error($"Unable to serialize {file} as tracked issue file ", ex);
            }
        }
    }
}