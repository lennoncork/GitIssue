using System.Collections.Generic;

namespace GitIssue.Keys
{
    public interface IIssueKeyProvider
    {
        bool TryGetKey(string value, out IssueKey key);

        IEnumerable<IssueKey> Keys { get; }

        IssueKey Next();
    }
}
