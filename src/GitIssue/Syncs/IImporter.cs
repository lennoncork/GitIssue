using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitIssue.Syncs
{
    /// <summary>
    /// Importing interface
    /// </summary>
    public interface IImporter : IAsyncEnumerable<SyncedIssue>
    {
        /// <summary>
        /// Gets the Root
        /// </summary>
        public SyncRoot Root { get; }

        /// <summary>
        /// Imports the issues
        /// </summary>
        /// <returns></returns>
        public Task<bool> Import();
    }
}