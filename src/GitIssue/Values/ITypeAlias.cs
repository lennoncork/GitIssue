using System;

namespace GitIssue.Values
{
    public interface ITypeAlias
    {
        public bool TryParse(string alias, out Type type);

        public bool TryParse(Type type, out string alias);
    }
}