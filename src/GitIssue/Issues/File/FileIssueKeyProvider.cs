using System;
using GitIssue.Values;

namespace GitIssue.Issues.File
{
    /// <summary>
    ///     The date-based issue key provider
    /// </summary>
    /// <remarks>
    ///     Retained so existing configuration files that name this type continue to load.
    ///     Use <see cref="DateIssueKeyProvider" /> or a <see cref="IssueConfiguration.KeyTemplate" /> for new repositories.
    /// </remarks>
    [Obsolete("Use DateIssueKeyProvider or TemplateIssueKeyProvider instead.")]
    [TypeAlias(nameof(FileIssueKeyProvider))]
    public class FileIssueKeyProvider : DateIssueKeyProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        public FileIssueKeyProvider(RepositoryRoot root)
            : base(root)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="configuration">the issue configuration</param>
        public FileIssueKeyProvider(RepositoryRoot root, IIssueConfiguration configuration)
            : base(root, configuration)
        {
        }
    }
}
