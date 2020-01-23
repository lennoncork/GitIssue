using System.Collections.Generic;

namespace GitIssue.Providers
{
    public interface IIssueKeyProvider
    {
        bool TryGetKey(string value, out IssueKey key);

        IEnumerable<IssueKey> Keys { get; }

        IssueKey Next();
    }
}
