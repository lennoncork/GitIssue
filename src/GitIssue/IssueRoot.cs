﻿using System.IO;
using GitIssue.Keys;

namespace GitIssue
{
    /// <summary>
    /// Specifies the location on disk of a given issue
    /// </summary>
    public class IssueRoot
    {
        /// <summary>
        /// Gets the <see cref="RepositoryRoot"/>
        /// </summary>
        public RepositoryRoot Root { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IssueKey"/> for the issue
        /// </summary>
        public IssueKey Key { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IssuePath"/> for the issue
        /// </summary>
        public string IssuePath => Path.Combine(this.Root.IssuesPath, this.Key.ToString());

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueRoot"/> class.
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="key">this issue key</param>
        public IssueRoot(RepositoryRoot root, IssueKey key)
        {
            this.Root = root;
            this.Key = key;
        }

    }
}