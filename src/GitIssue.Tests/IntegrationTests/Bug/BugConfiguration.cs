using GitIssue.Fields;
using GitIssue.Json;
using GitIssue.Keys;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    public class BugConfiguration : IssueConfiguration
    {
        public BugConfiguration()
        {
            Fields.Add(FieldKey.Create("FixVersion"), new FieldInfo<string>
            {
                FieldType = FieldType.Create<JsonArrayField>(),
                Required = true
            });
        }
    }
}