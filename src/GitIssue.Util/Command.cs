using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssue.Util
{
    /// <summary>
    /// The GitIssueCommand
    /// </summary>
    public enum Command
    {
        /// <summary>
        /// No command, default
        /// </summary>
        None,

        /// <summary>
        /// Initialize the issues
        /// </summary>
        Init,

        /// <summary>
        /// Creates a new issue
        /// </summary>
        Create,

        /// <summary>
        /// Deletes and existing issue
        /// </summary>
        Delete,

        /// <summary>
        /// Finds an existing issue
        /// </summary>
        Find,
    }
}
