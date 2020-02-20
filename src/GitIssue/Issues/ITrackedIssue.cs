using System;
using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Issues
{
    /// <summary>
    /// Interface for the tracked issue
    /// </summary>
    public interface ITrackedIssue
    {
        /// <summary>
        ///     Gets the key of the tracked issue
        /// </summary>
        IssueKey Key { get; set; }

        /// <summary>
        ///     Gets the started date of the tracking
        /// </summary>
        DateTime Started { get; set; }

        /// <summary>
        ///     Saves the configuration to a file
        /// </summary>
        /// <param name="file">the configuration file</param>
        /// <param name="logger">the logger</param>
        Task SaveAsync(string file, ILogger? logger = null);
    }
}