using System.Collections.Generic;
using GitIssue.Fields;

namespace GitIssue
{
    /// <summary>
    ///     The field schema associated with an issue type.
    /// </summary>
    public sealed class IssueTypeConfiguration
    {
        /// <summary>
        ///     Gets or sets the issue type identifier.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the fields belonging to this issue type.
        /// </summary>
        public Dictionary<FieldKey, FieldInfo> Fields { get; set; } = new Dictionary<FieldKey, FieldInfo>();
    }
}
