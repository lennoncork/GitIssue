using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Values;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    /// <summary>
    ///     Extension methods for the 'Bug' issue type
    /// </summary>
    public static class BugExtensions
    {
        private static readonly FieldKey FixVersionKey = FieldKey.Create("FixVersion");
        private static readonly FieldKey SeverityKey = FieldKey.Create("Severity");

        /// <summary>
        ///     Gets the fix version of the issue
        /// </summary>
        /// <param name="issue">the issue</param>
        public static Version[] GetFixVersion(this IIssue issue)
        {
            Version[] fixVersion = issue.GetField(BugExtensions.FixVersionKey).AsArray<Version>();
            return fixVersion;
        }

        /// <summary>
        ///     Gets the severity of the issue
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <returns></returns>
        public static Enumerated GetSeverity(this IIssue issue)
        {
            Enumerated severity = issue.GetField(BugExtensions.SeverityKey).AsValue<Enumerated>();
            return severity;
        }

        /// <summary>
        ///     Sets the fix version of the issue
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="fixVersion">the fix version to set</param>
        public static void SetFixVersion(this IIssue issue, Version[] fixVersion)
        {
            issue.SetField(BugExtensions.FixVersionKey).WithArray(fixVersion);
        }

        /// <summary>
        ///     Sets the severity of the issue
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="severity">the issue's severity</param>
        public static void SetSeverity(this IIssue issue, Enumerated severity)
        {
            issue.SetField(BugExtensions.SeverityKey).WithValue(severity);
        }
    }
}