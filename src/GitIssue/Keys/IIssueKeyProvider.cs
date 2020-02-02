using System.Collections.Generic;

namespace GitIssue.Keys
{
    public interface IIssueKeyProvider
    {
        IEnumerable<IssueKey> Keys { get; }
        bool TryGetKey(string value, out IssueKey key);

        IssueKey Next();
    }
}