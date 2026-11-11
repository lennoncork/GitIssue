using System;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Services;
using NUnit.Framework;

namespace GitIssue.Services.Web.Tests
{
    [TestFixture]
    public class IssueSearchTests
    {
        [Test]
        public async Task SearchIssues_PagesAndFiltersByKeyTitleAndDescription()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueWorkspace workspace = fixture.Open();
            IssueDocument first = await workspace.CreateIssueAsync("Alpha issue", "First description", CancellationToken.None);
            await workspace.CreateIssueAsync("Beta issue", "Needle description", CancellationToken.None);
            await workspace.CreateIssueAsync("Another issue", "Other", CancellationToken.None);

            IssuePage page = await workspace.SearchIssuesAsync("issue", 2, 2, CancellationToken.None);
            IssuePage title = await workspace.SearchIssuesAsync("ALPHA", 1, 10, CancellationToken.None);
            IssuePage description = await workspace.SearchIssuesAsync("needle", 1, 10, CancellationToken.None);
            IssuePage key = await workspace.SearchIssuesAsync(first.Key, 1, 10, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(page.Total, Is.EqualTo(3));
                Assert.That(page.Items, Has.Count.EqualTo(1));
                Assert.That(title.Items[0].Key, Is.EqualTo(first.Key));
                Assert.That(description.Items[0].Title, Is.EqualTo("Beta issue"));
                Assert.That(key.Items[0].Key, Is.EqualTo(first.Key));
            });
        }

        [Test]
        public async Task SearchIssues_ReturnsMatchesBeyondTheListLimit()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueWorkspace workspace = fixture.Open();
            for (int index = 0; index < 201; index++)
            {
                await workspace.CreateIssueAsync($"Issue {index}", string.Empty, CancellationToken.None);
            }

            IssuePage result = await workspace.SearchIssuesAsync(null, 21, 10, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Total, Is.EqualTo(201));
                Assert.That(result.Items, Has.Count.EqualTo(1));
            });
        }

        [TestCase(0, 10)]
        [TestCase(1, 0)]
        [TestCase(1, 201)]
        public void SearchIssues_RejectsInvalidPaging(int page, int pageSize)
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueWorkspace workspace = fixture.Open();

            Assert.That(
                async () => await workspace.SearchIssuesAsync(null, page, pageSize, CancellationToken.None),
                Throws.TypeOf<IssueOperationException>());
        }

        [Test]
        public async Task SearchIssues_HandlesPageOffsetsBeyondIntegerRange()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IssueWorkspace workspace = fixture.Open();
            await workspace.CreateIssueAsync("One", string.Empty, CancellationToken.None);

            IssuePage page = await workspace.SearchIssuesAsync(null, int.MaxValue, 200, CancellationToken.None);

            Assert.That(page.Items, Is.Empty);
        }
    }
}
