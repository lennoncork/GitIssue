using System.Collections.Generic;
using GitIssue.Fields;

namespace GitIssue
{
    /// <summary>
    ///     The issue configuration interface
    /// </summary>
    public interface IIssueConfiguration
    {
        /// <summary>
        ///     Gets or sets the dictionary of fields
        /// </summary>
        Dictionary<FieldKey, FieldInfo> Fields { get; set; }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        void Save(string file);

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <typeparam name="T">the configuration type</typeparam>
        /// <param name="file">the configuration file</param>
        void Save<T>(string file) where T : IssueConfiguration, new();
    }
}