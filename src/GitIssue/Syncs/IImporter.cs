using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitIssue.Syncs
{
    /// <summary>
    ///     Importing interface
    /// </summary>
    public interface IImporter : IAsyncEnumerable<SyncedIssue>
    {
        /// <summary>
        ///     Gets the Root
        /// </summary>
        SyncRoot Root { get; }

        /// <summary>
        ///     Imports the issues
        /// </summary>
        /// <returns></returns>
        Task<bool> Import();
    }
}