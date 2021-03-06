﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Issues;
using LibGit2Sharp;

namespace GitIssue
{
    /// <summary>
    ///     Interface for the issue manager/repository
    /// </summary>
    public interface IIssueManager : IAsyncDisposable, IDisposable
    {
        /// <summary>
        ///     Gets the working directory
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        ///     Gets the repository root
        /// </summary>
        public RepositoryRoot Root { get; }

        /// <summary>
        ///     Gets the GIT repository
        /// </summary>
        IRepository Repository { get; }

        /// <summary>
        ///     Gets the issue configuration
        /// </summary>
        IIssueConfiguration Configuration { get; }

        /// <summary>
        ///     Gets the change log for the issues
        /// </summary>
        IChangeLog Changes { get; }

        /// <summary>
        /// Gets the tracked issue
        /// </summary>
        ITrackedIssue Tracked { get; }

        /// <summary>
        /// Gets the issue key provider
        /// </summary>
        IIssueKeyProvider KeyProvider { get; }

        /// <summary>
        ///     Commits the changes
        /// </summary>
        /// <returns></returns>
        bool Commit();

        /// <summary>
        ///     Commits the changes
        /// </summary>
        /// <returns></returns>
        Task<bool> CommitAsync();

        /// <summary>
        ///     Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <returns></returns>
        IIssue Create(string title);

        /// <summary>
        ///     Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <param name="description">the issue description</param>
        /// <returns></returns>
        IIssue Create(string title, string description);

        /// <summary>
        ///     Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <returns></returns>
        Task<IIssue> CreateAsync(string title);

        /// <summary>
        ///     Creates a new issue
        /// </summary>
        /// <param name="title">the issue title</param>
        /// <param name="description">the issue description</param>
        /// <returns></returns>
        Task<IIssue> CreateAsync(string title, string description);

        /// <summary>
        ///     Finds an issue
        /// </summary>
        /// <param name="predicated">the predicate to evaluate</param>
        /// <returns></returns>
        IEnumerable<IIssue> Find(Func<IIssue, bool> predicated);

        /// <summary>
        ///     Finds an issue asynchronously.
        /// </summary>
        /// <param name="predicated">the predicate to evaluate the issue against</param>
        /// <returns></returns>
        IAsyncEnumerable<IIssue> FindAsync(Func<IIssue, bool> predicated);

        /// <summary>
        ///     Deletes an existing issue
        /// </summary>
        /// <param name="id">the id to search</param>
        bool Delete(string id);

        /// <summary>
        ///     Deletes an existing issue
        /// </summary>
        /// <param name="key">the issue key</param>
        bool Delete(IssueKey key);

        /// <summary>
        ///     Deletes an existing issue
        /// </summary>
        /// <param name="id">the id to search</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        ///     Deletes an existing issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(IssueKey key);

        /// <summary>
        ///     Tracks the issue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Track(IssueKey key);

        /// <summary>
        ///     Tracks the issue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> TrackAsync(IssueKey key);

    }
}