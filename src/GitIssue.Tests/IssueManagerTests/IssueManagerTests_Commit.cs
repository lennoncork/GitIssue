using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Commit : IssueManagerTests
        {
            [Test]
            public async Task Commit_ReturnsTrueAndClearsChangeLog()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.Commit_ReturnsTrueAndClearsChangeLog), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.That(this.Sut.Changes.Log, Is.Not.Empty);

                bool result = this.Sut.Commit();

                Assert.That(result, Is.True);
                Assert.That(this.Sut.Changes.Log, Is.Empty);
            }

            [Test]
            public async Task CommitAsync_ReturnsTrueWhenNoStagedChanges()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_ReturnsTrueWhenNoStagedChanges), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
            }
        }
    }
}
