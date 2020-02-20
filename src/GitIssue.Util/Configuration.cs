using Newtonsoft.Json;

namespace GitIssue.Util
{
    /// <summary>
    ///     Configuration for the issue
    /// </summary>
    public class Configuration : IssueConfiguration
    {
        /// <summary>
        ///     Gets or sets the editor to use
        /// </summary>
        [JsonProperty]
        public string Editor { get; set; } = "joe";

        /// <summary>
        ///     Gets or sets additional arguments for the editor
        /// </summary>
        [JsonProperty]
        public string Arguments { get; set; } = "-pound_comment -syntax git-commit";

        /// <summary>
        ///     Reads the configuration from a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <returns>the <see cref="Configuration" /></returns>
        public new static Configuration Read(string file)
        {
            return Read<Configuration>(file);
        }
    }
}