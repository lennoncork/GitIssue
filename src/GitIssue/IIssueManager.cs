using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Keys;
using LibGit2Sharp;

namespace GitIssue
{
    /// <summary>
    /// Interface for the issue manager/repository
    /// </summary>
    public interface IIssueManager : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the working directory
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Gets the GIT repository
        /// </summary>
        IRepository Repository { get; }

        /// <summary>
        /// Gets the issue configuration
        /// </summary>
        IssueConfiguration Configuration { get; }

        /// <summary>
        /// Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <returns></returns>
        IIssue Create(string title);

        /// <summary>
        /// Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <param name="description">the issue description</param>
        /// <returns></returns>
        IIssue Create(string title, string description);

        /// <summary>
        /// Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <returns></returns>
        Task<IIssue> CreateAsync(string title);

        /// <summary>
        /// Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <param name="description">the issue description</param>
        /// <returns></returns>
        Task<IIssue> CreateAsync(string title, string description);

        /// <summary>
        /// Finds an issue
        /// </summary>
        /// <param name="predicated">the predicate to evaluate</param>
        /// <returns></returns>
        IEnumerable<IIssue> Find(Func<IIssue, bool> predicated);

        /// <summary>
        /// Finds an issue asynchronously. 
        /// </summary>
        /// <param name="predicated">the predicate to evaluate the issue against</param>
        /// <returns></returns>
        IAsyncEnumerable<IIssue> FindAsync(Func<IIssue, bool> predicated);

        /// <summary>
        /// Deletes an existing issue
        /// </summary>
        /// <param name="id">the id to search</param>
        void Delete(string id);

        /// <summary>
        /// Deletes an existing issue
        /// </summary>
        /// <param name="key">the issue key</param>
        void Delete(IssueKey key);

        /// <summary>
        /// Deletes an existing issue
        /// </summary>
        /// <param name="id">the id to search</param>
        /// <returns></returns>
        Task DeleteAsync(string id);

        /// <summary>
        /// Deletes an existing issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <returns></returns>
        Task DeleteAsync(IssueKey key);

    }
    
}
