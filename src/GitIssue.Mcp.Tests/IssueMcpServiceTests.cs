using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Mcp.Tools;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Mcp.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class IssueMcpServiceTests
    {
        [Test]
        public async Task Create_PersistsIssueWithoutCommitting()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueMcpService service = fixture.Open();

            IssueDocument created = await service.CreateIssueAsync("MCP title", "MCP description", CancellationToken.None);
            IssueDocument loaded = await service.GetIssueAsync(created.Key, CancellationToken.None);

            using Repository repository = new Repository(fixture.Directory);
            Assert.Multiple(() =>
            {
                Assert.That(repository.Info.IsHeadUnborn, Is.True);
                Assert.That(loaded.Title, Is.EqualTo("MCP title"));
                Assert.That(loaded.Description, Is.EqualTo("MCP description"));
                Assert.That(loaded.Author, Does.Contain("Tester"));
                Assert.That(loaded.Fields.Keys, Does.Contain("Title"));
                Assert.That(service.GetChanges().Any(change => change.Key == created.Key), Is.True);
            });
        }

        [Test]
        public async Task FindAndUpdate_RoundTripsConfiguredFields()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueMcpService service = fixture.Open();
            IssueDocument created = await service.CreateIssueAsync("Feature work", "Expose issues.", CancellationToken.None);

            IssueDocument updated = await service.UpdateIssueAsync(created.Key, "Labels", "+feature", CancellationToken.None);
            IssueDocument commented = await service.AddCommentAsync(created.Key, "Ready for agents", CancellationToken.None);
            TrackedIssueDocument tracked = await service.TrackIssueAsync(created.Key, CancellationToken.None);
            IReadOnlyList<IssueDocument> found = await service.FindIssuesAsync(
                new IssueQuery
                {
                    TitleContains = "feature",
                    Field = "Labels",
                    FieldEquals = "feature",
                },
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(updated.Fields["Labels"].ToString(), Does.Contain("feature"));
                Assert.That(commented.Comments, Does.Contain("Ready for agents"));
                Assert.That(tracked.Key, Is.EqualTo(created.Key));
                Assert.That(found.Select(issue => issue.Key), Is.EqualTo(new[] { created.Key }));
                Assert.That(service.ListFields().Any(field => (field.Name == "Title") && field.Required), Is.True);
            });
        }

        [Test]
        public void GetIssue_MissingKey_Throws()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueMcpService service = fixture.Open();

            Assert.That(
                async () => await service.GetIssueAsync("MISSING-1", CancellationToken.None),
                Throws.TypeOf<McpOperationException>());
        }

        [Test]
        public async Task Tools_ReadAndWriteThroughTheSession()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueMcpService service = fixture.Open();
            IssueTools tools = new IssueTools(new IssueSession(() => service));
            IssueResources resources = new IssueResources(new IssueSession(() => service));

            IssueDocument created = await tools.CreateIssue("Listed issue", "Described work", CancellationToken.None);
            await tools.UpdateIssue(created.Key, "Description", "Updated description", CancellationToken.None);
            IReadOnlyList<IssueSummary> listed = await tools.ListIssues(1, CancellationToken.None);
            IssueDocument loaded = await tools.GetIssue(created.Key, CancellationToken.None);
            IReadOnlyList<IssueDocument> found = await tools.FindIssues("listed", "updated", null, null, 10, CancellationToken.None);
            string issueJson = await resources.Issue(created.Key, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(listed, Has.Count.EqualTo(1));
                Assert.That(listed[0].Key, Is.EqualTo(created.Key));
                Assert.That(listed[0].Title, Is.EqualTo("Listed issue"));
                Assert.That(listed[0].Description, Is.EqualTo("Updated description"));
                Assert.That(loaded.Description, Is.EqualTo("Updated description"));
                Assert.That(found, Has.Count.EqualTo(1));
                Assert.That(tools.ListFields().Any(field => field.Name == "Description"), Is.True);
                Assert.That(tools.GetTrackedIssue().Key, Is.EqualTo("None"));
                Assert.That(tools.GetChanges().Any(change => change.Key == created.Key), Is.True);
                Assert.That(issueJson, Does.Contain(created.Key));
                Assert.That(resources.Schema(), Does.Contain("Comments"));
            });
        }

        [Test]
        public async Task Mutations_RejectInvalidInput()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueMcpService service = fixture.Open();
            IssueDocument created = await service.CreateIssueAsync("Valid", null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(
                    async () => await service.CreateIssueAsync("  ", "Body", CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
                Assert.That(
                    async () => await service.ListIssuesAsync(0, CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
                Assert.That(
                    async () => await service.GetIssueAsync(" ", CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
                Assert.That(
                    async () => await service.UpdateIssueAsync(created.Key, " ", "value", CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
                Assert.That(
                    async () => await service.UpdateIssueAsync(created.Key, "Missing", "value", CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
                Assert.That(
                    async () => await service.UpdateIssueAsync(created.Key, "Labels", "feature", CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
                Assert.That(
                    async () => await service.AddCommentAsync(created.Key, " ", CancellationToken.None),
                    Throws.TypeOf<McpOperationException>());
            });
        }

        [Test]
        public void IssueSession_RethrowsMcpOperationException()
        {
            IssueSession session = new IssueSession(() => throw new McpOperationException("already reported"));

            McpOperationException? exception = Assert.Throws<McpOperationException>(() => session.Open());

            Assert.That(exception!.Message, Is.EqualTo("already reported"));
        }

        [Test]
        public async Task UpdateIssue_RejectsReadOnlyKey()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueMcpService service = fixture.Open();
            IssueDocument created = await service.CreateIssueAsync("Keep the key", "Body", CancellationToken.None);

            Assert.That(
                async () => await service.UpdateIssueAsync(created.Key, "Key", "OTHER-1", CancellationToken.None),
                Throws.TypeOf<McpOperationException>());
        }
    }
}
