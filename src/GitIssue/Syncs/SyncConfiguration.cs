using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitIssue.Syncs
{
    /// <summary>
    ///     Base class for importing issues
    /// </summary>
    public class SyncConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SyncConfiguration" /> class
        /// </summary>
        public SyncConfiguration()
        {
            this.Mapping = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Gets or sets the mapping between issue field and imported JSON
        /// </summary>
        public Dictionary<string, string> Mapping { get; set; }

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="SyncConfiguration" /></returns>
        public static Task<SyncConfiguration> ReadAsync(string file)
        {
            return SyncConfiguration.ReadAsync<SyncConfiguration>(file);
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
                await using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                return await JsonSerializer.DeserializeAsync<T>(stream, JsonSerialization.Options)
                    ?? throw new JsonException($"Unable to deserialize {file} as config file");
            }
            catch (Exception ex)
            {
                throw new AggregateException($"Unable to deserialize {file} as config file ", ex);
            }
        }
    }
}