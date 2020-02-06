using GitIssue.Keys;
using GitIssue.Values;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    public static class BugExtensions
    {
        private static readonly FieldKey Key = FieldKey.Create("FixVersion");

        public static void SetFixVersion(this IIssue issue, Version[] fixVersion)
        {
            issue.SetField(Key).WithArray(fixVersion);
        }

        public static Version[] GetFixVersion(this IIssue issue)
        {
            var fixVersion = issue.GetField(Key).AsArray<Version>();
            return fixVersion;
        }
    }
}