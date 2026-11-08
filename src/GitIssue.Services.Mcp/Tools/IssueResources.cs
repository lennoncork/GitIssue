using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Services;
using ModelContextProtocol.Server;

namespace GitIssue.Services.Mcp.Tools
{
    /// <summary>
    ///     MCP resources for the issue schema and one issue document
    /// </summary>
    [McpServerResourceType]
    public sealed class IssueResources
    {
        private readonly IssueSession session;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueResources" /> class
        /// </summary>
        /// <param name="session">the issue session</param>
        public IssueResources(IssueSession session)
        {
            this.session = session;
        }

        /// <summary>
        ///     Reads the configured fields
        /// </summary>
        /// <returns>the schema JSON</returns>
        [McpServerResource(UriTemplate = "gitissue://schema", Name = "schema", MimeType = "application/json")]
        [Description("The configured issue fields. Features are described by these fields.")]
        public string Schema()
        {
            return IssueDocumentMapper.Serialize(this.session.Open().ListFields());
        }

        /// <summary>
        ///     Reads one issue
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>the issue JSON</returns>
        [McpServerResource(UriTemplate = "gitissue://issue/{key}", Name = "issue", MimeType = "application/json")]
        [Description("One issue document, including comments and configured fields.")]
        public async Task<string> Issue(string key, CancellationToken cancellationToken = default)
        {
            IssueDocument document = await this.session.Open().GetIssueAsync(key, cancellationToken);
            return IssueDocumentMapper.Serialize(document);
        }
    }
}
