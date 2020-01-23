using System.Collections.Generic;
using System.IO;
using GitIssue.Fields;
using GitIssue.Json;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    public class BugConfiguration : IssueConfiguration
    {
        public BugConfiguration() : base()
        {
            this.Fields.Add(FieldKey.Create("FixVersion"), new FieldInfo<string>
            {
                FieldType = FieldType.Create<JsonArrayField>(),
                Required = true
            });
        }
    }
}