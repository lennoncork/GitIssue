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
        }
    }
}