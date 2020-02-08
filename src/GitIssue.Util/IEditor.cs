using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Fields;

namespace GitIssue.Editors
{
    public interface IEditor
    {
        /// <summary>
        /// Opens an issue for editing
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        Task Open(IIssue issue);

        /// <summary>
        /// Opens a field for editing
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        Task Open(IField field);
    }
}
