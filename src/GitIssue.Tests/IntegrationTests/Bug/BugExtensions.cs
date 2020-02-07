using GitIssue.Fields;
using GitIssue.Keys;
using GitIssue.Values;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    public static class BugExtensions
    {
        private static readonly FieldKey FixVersionKey = FieldKey.Create("FixVersion");
        private static readonly FieldKey SeverityKey = FieldKey.Create("Severity");

        public static void SetFixVersion(this IIssue issue, Version[] fixVersion)
        {
            issue.SetField(FixVersionKey).WithArray(fixVersion);
        }

        public static Version[] GetFixVersion(this IIssue issue)
        {
            var fixVersion = issue.GetField(FixVersionKey).AsArray<Version>();
            return fixVersion;
        }

        public static void SetSeverity(this IIssue issue, Enumerated severity)
        {
            issue.SetField(SeverityKey).WithValue(severity);
        }

        public static Enumerated GetSeverity(this IIssue issue)
        {
            var severity = issue.GetField(SeverityKey).AsValue<Enumerated>();
            return severity;
        }
    }
}