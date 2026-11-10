using GitIssue.Values;
using LibGit2Sharp;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Generates date-based issue keys from {created:yyyy}/{created:MM}/{created:dd}/{uid}
    /// </summary>
    [TypeAlias(nameof(DateIssueKeyProvider))]
    public class DateIssueKeyProvider : TemplateIssueKeyProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DateIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        public DateIssueKeyProvider(RepositoryRoot root)
            : this(root, DateIssueKeyProvider.createFallbackConfiguration())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="configuration">the issue configuration</param>
        public DateIssueKeyProvider(RepositoryRoot root, IIssueConfiguration configuration)
            : base(root, configuration, IssueKeyTemplate.Date)
        {
        }

        /// <summary>
        ///     Tries to get the commit of the current branch
        /// </summary>
        /// <param name="commit"></param>
        /// <returns></returns>
        public bool TryGetGitCommit(out string commit)
        {
            using IRepository repository = this.Root.GetRepository();
            commit = repository.Head.Tip.Sha;
            return true;
        }

        private static IssueConfiguration createFallbackConfiguration()
        {
            return new IssueConfiguration
            {
                KeyTemplate = IssueKeyTemplate.Date,
            };
        }
    }
}
