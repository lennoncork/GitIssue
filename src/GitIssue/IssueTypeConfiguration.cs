using System.Collections.Generic;
using System.Text.Json.Serialization;
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
        ///     Gets or sets how this type stores its files. Template uses the key template.
        ///     Free keeps the key in the document and allows any folder under the type directory.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IssueLayout Layout { get; set; } = IssueLayout.Template;

        /// <summary>
        ///     Gets or sets the fields belonging to this issue type.
        /// </summary>
        public Dictionary<FieldKey, FieldInfo> Fields { get; set; } = new Dictionary<FieldKey, FieldInfo>();
    }
}
