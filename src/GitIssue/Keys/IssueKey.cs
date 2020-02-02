using System;

namespace GitIssue.Keys
{
    public struct IssueKey : IEquatable<IssueKey>
    {
        public readonly string key;

        private IssueKey(string key)
        {
            this.key = key;
        }

        public static IssueKey Create(string key) => new IssueKey(key);

        public static IssueKey None() => new IssueKey(string.Empty);

        public static bool operator ==(IssueKey x, IssueKey y) => x.key == y.key;

        public static bool operator !=(IssueKey x, IssueKey y) => !(x == y);

        public override string ToString() => key;

        public override int GetHashCode() => key.GetHashCode();

        public override bool Equals(object obj) => key.Equals(obj);

        public bool Equals( IssueKey other) => this.key == other.key;
    }
}
