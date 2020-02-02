using GitIssue.Keys;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    public static class BugExtensions
    {
        private static readonly FieldKey Key = FieldKey.Create("FixVersion");

        public static void SetFixVersion(this IIssue issue, string[] fixVersion)
        {
            issue.SetField(Key).WithArray(fixVersion);
        }

        public static string[] GetFixVersion(this IIssue issue)
        {
            var fixVersion = issue.GetField(Key).AsArray<string>();
            return fixVersion;
        }
    }
}