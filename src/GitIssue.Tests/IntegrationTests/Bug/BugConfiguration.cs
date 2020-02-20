using GitIssue.Fields;
using GitIssue.Issues.Json;
using GitIssue.Values;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    /// <summary>
    ///     A defect configuration
    /// </summary>
    public class BugConfiguration : IssueConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BugConfiguration" /> class
        /// </summary>
        public BugConfiguration()
        {
            Fields.Add(FieldKey.Create("AffectsVersion"), new FieldInfo<Version, JsonArrayField>(false));
            Fields.Add(FieldKey.Create("FixVersion"), new FieldInfo<Version, JsonArrayField>(false));
            Fields.Add(FieldKey.Create("Severity"), new FieldInfo<Enumerated, JsonValueField>(false)
            {
                ValueMetadata = "[S1, S1, S3, S4, S5]"
            });
        }
    }
}