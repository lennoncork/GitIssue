using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Tool
{
    /// <summary>
    ///     Editor interface
    /// </summary>
    public interface IEditor
    { 
        /// <summary>
        /// Edits existing content
        /// </summary>
        /// <param name="header">the header</param>
        /// <param name="content">the content</param>
        /// <returns></returns>
        Task<string> Edit(string header, string content);

        /// <summary>
        ///     Opens an issue for editing
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        Task Open(IIssue issue);

        /// <summary>
        ///     Opens an collection of fields for editing
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        Task Open(IEnumerable<IField> fields);

        /// <summary>
        ///     Opens a field for editing
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        Task Open(IField field);
    }
}