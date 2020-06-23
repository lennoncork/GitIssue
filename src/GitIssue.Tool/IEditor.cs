using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Util
{
    /// <summary>
    ///     Editor interface
    /// </summary>
    public interface IEditor
    {
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