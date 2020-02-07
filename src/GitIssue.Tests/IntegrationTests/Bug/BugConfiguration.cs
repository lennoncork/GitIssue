using GitIssue.Fields;
using GitIssue.Json;
using GitIssue.Keys;
using GitIssue.Values;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    public class BugConfiguration : IssueConfiguration
    {
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