using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GitIssue.Syncs
{
    /// <summary>
    /// Base class for importing issues
    /// </summary>
    public class SyncConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class
        /// </summary>
        public SyncConfiguration()
        {

            this.Mapping = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the mapping between issue field and imported JSON
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Mapping { get; set; }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="SyncConfiguration" /></returns>
        public static Task<SyncConfiguration> ReadAsync(string file)
        {
            return ReadAsync<SyncConfiguration>(file);
        }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="SyncConfiguration" /></returns>
        public static async Task<T> ReadAsync<T>(string file) where T : SyncConfiguration, new()
        {
            try
            {
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
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

