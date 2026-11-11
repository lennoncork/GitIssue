using GitIssue.Values;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Generates named issue keys from {uid} - {name}
    /// </summary>
    [TypeAlias(nameof(NameIssueKeyProvider))]
    public class NameIssueKeyProvider : TemplateIssueKeyProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NameIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        public NameIssueKeyProvider(RepositoryRoot root)
            : this(root, NameIssueKeyProvider.createFallbackConfiguration())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NameIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="configuration">the issue configuration</param>
        public NameIssueKeyProvider(RepositoryRoot root, IIssueConfiguration configuration)
            : base(root, configuration, IssueKeyTemplate.Name)
        {
        }

        private static IssueConfiguration createFallbackConfiguration()
        {
            return new IssueConfiguration
            {
                KeyProvider = TypeValue.Create<NameIssueKeyProvider>(),
                KeyTemplate = IssueKeyTemplate.Name,
            };
        }
    }
}
