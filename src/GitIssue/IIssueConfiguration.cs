using System.Collections.Generic;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue
{
    /// <summary>
    ///     The issue configuration interface
    /// </summary>
    public interface IIssueConfiguration
    {
        /// <summary>
        ///     Gets or sets the configured issue type schemas.
        /// </summary>
        List<IssueTypeConfiguration> Issues { get; set; }

        /// <summary>
        ///     Gets the fields configured for an issue type.
        /// </summary>
        /// <param name="issueType">the issue type identifier</param>
        /// <returns>the issue type's field schema</returns>
        Dictionary<FieldKey, FieldInfo> GetFields(string issueType);

        /// <summary>
        ///     Gets or sets the key provider
        /// </summary>
        TypeValue KeyProvider { get; set; }

        /// <summary>
        ///     Gets or sets the issue type token used by {type} in the key template
        /// </summary>
        string IssueType { get; set; }

        /// <summary>
        ///     Gets or sets the issue key template
        /// </summary>
        string KeyTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the on-disk format used for issue documents
        /// </summary>
        IssueStorage Storage { get; set; }

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
