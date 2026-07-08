using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class IssuesTests
    {
        private static IEnumerable<Type> IssueTypes()
        {
            yield return typeof(JsonIssue);
        }

        private static async Task<IIssue> CreateIssueAsync(Type issueType)
        {
            string repoDir = Helpers.CreateTempDirectory();
            RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
            IssueKey issueKey = IssueKey.Create("ISS-ISSUE-1");
            IssueRoot issueRoot = new IssueRoot(repoRoot, issueKey, "issue1");

            // Build fields from default configuration so all standard fields exist
            IssueConfiguration configuration = new IssueConfiguration();
            IDictionary<FieldKey, FieldInfo> fields = configuration.Fields;

            if (issueType == typeof(JsonIssue))
            {
                JsonIssue? issue = new JsonIssue(issueRoot, fields);
                return issue;
            }

            throw new ArgumentOutOfRangeException(nameof(issueType));
        }

        [TestFixture]
        public class Properties : IssuesTests
        {
            [TestCaseSource(typeof(IssuesTests), nameof(IssueTypes))]
            public async Task Author_Can_Be_Set_And_Read(Type issueType)
            {
                IIssue issue = await CreateIssueAsync(issueType);
                Signature author = Signature.Parse("Author Name <author@example.com>");

                issue.Author = author;

                Assert.That(issue.Author, Is.EqualTo(author));
            }

            [TestCaseSource(typeof(IssuesTests), nameof(IssueTypes))]
            public async Task Created_Can_Be_Set_And_Read(Type issueType)
            {
                IIssue issue = await CreateIssueAsync(issueType);
                GitIssue.Values.DateTime created = (GitIssue.Values.DateTime)System.DateTime.Parse("2020-01-02T03:04:05");

                issue.Created = created;

                Assert.That(issue.Created, Is.EqualTo(created));
            }

            [TestCaseSource(typeof(IssuesTests), nameof(IssueTypes))]
            public async Task Description_Can_Be_Set_And_Read(Type issueType)
            {
                IIssue issue = await CreateIssueAsync(issueType);
                GitIssue.Values.String description = GitIssue.Values.String.Parse("Test description");

                issue.Description = description;

                Assert.That(issue.Description, Is.EqualTo(description));
            }

            [TestCaseSource(typeof(IssuesTests), nameof(IssueTypes))]
            public async Task Title_Can_Be_Set_And_Read(Type issueType)
            {
                IIssue issue = await CreateIssueAsync(issueType);
                GitIssue.Values.String title = GitIssue.Values.String.Parse("Test title");

                issue.Title = title;

                Assert.That(issue.Title, Is.EqualTo(title));
            }

            [TestCaseSource(typeof(IssuesTests), nameof(IssueTypes))]
            public async Task Updated_Can_Be_Set_And_Read(Type issueType)
            {
                IIssue issue = await CreateIssueAsync(issueType);
                GitIssue.Values.DateTime updated = (GitIssue.Values.DateTime)System.DateTime.Parse("2020-02-03T04:05:06");

                issue.Updated = updated;

                Assert.That(issue.Updated, Is.EqualTo(updated));
            }

            [TestCaseSource(typeof(IssuesTests), nameof(IssueTypes))]
            public async Task Comments_Can_Be_Set_And_Read(Type issueType)
            {
                IIssue issue = await CreateIssueAsync(issueType);
                GitIssue.Values.String[] comments =
                {
                    GitIssue.Values.String.Parse("First comment"),
                    GitIssue.Values.String.Parse("Second comment"),
                };

                issue.Comments = comments;

                Assert.That(issue.Comments, Is.EquivalentTo(comments));
            }
        }
    }
}
