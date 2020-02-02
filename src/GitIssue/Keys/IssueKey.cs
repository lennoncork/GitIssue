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

        public static IssueKey Create(string key)
        {
            return new IssueKey(key);
        }

        public static IssueKey None()
        {
            return new IssueKey(string.Empty);
        }

        public static bool operator ==(IssueKey x, IssueKey y)
        {
            return x.key == y.key;
        }

        public static bool operator !=(IssueKey x, IssueKey y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return key;
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return key.Equals(obj);
        }

        public bool Equals(IssueKey other)
        {
            return key == other.key;
        }
    }
}