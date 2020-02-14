using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Util
{
    /// <summary>
    /// Editor interface
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
        ///     Opens a field for editing
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        Task Open(IField field);
    }
}