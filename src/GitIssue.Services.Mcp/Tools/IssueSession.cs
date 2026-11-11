using System;
using Exceptionable;
using GitIssue.Services;

namespace GitIssue.Services.Mcp.Tools
{
    /// <summary>
    ///     Opens the issue service for one tool call
    /// </summary>
    public sealed class IssueSession
    {
        private readonly Func<IssueWorkspace> open;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueSession" /> class
        /// </summary>
        /// <param name="open">the callback that opens the issue service</param>
        public IssueSession(Func<IssueWorkspace> open)
        {
            this.open = open;
        }

        /// <summary>
        ///     Opens the issue service
        /// </summary>
        /// <returns>the issue service</returns>
        [Throws<IssueOperationException>]
        public IssueWorkspace Open()
        {
            try
            {
                return this.open();
            }
            catch (IssueOperationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new IssueOperationException(
                    "Unable to open the GitIssue repository. Run gitissue init and start the server from the repository root. " + exception.GetBaseException().Message,
                    exception);
            }
        }
    }
}
