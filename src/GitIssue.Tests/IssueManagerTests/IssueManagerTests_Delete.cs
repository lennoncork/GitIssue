using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Issues.Markdown;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;
using Serilog;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Delete : IssueManagerTests
        {
            [Test]
            public void DeleteById_DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = this.Sut.Create(nameof(Delete.DeleteById_DeletesExistingIssue), string.Empty);
                string path = Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(issue.Key));
                Assert.IsTrue(Directory.Exists(path));
                bool result = this.Sut.Delete(issue.Key.ToString());
                Assert.That(result, Is.True);
                Assert.IsFalse(Directory.Exists(path));
            }

            [Test]
            public async Task DeleteAsyncById_ReturnsTrueWhenDeletionSucceeds()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = this.Sut.Create(nameof(Delete.DeleteAsyncById_ReturnsTrueWhenDeletionSucceeds), string.Empty);
                string path = Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(issue.Key));
                Assert.IsTrue(Directory.Exists(path));
                bool result = await this.Sut.DeleteAsync(issue.Key.ToString());
                Assert.That(result, Is.True);
                Assert.IsFalse(Directory.Exists(path));
            }

            [Test]
            public void DeleteById_ReturnsFalseWhenKeyNotFound()
            {
                this.Initialize(this.TestDirectory);
                bool result = this.Sut.Delete("invalid-id");
                Assert.That(result, Is.False);
            }

            [Test]
            public async Task DeleteAsyncById_ReturnsFalseWhenDeletionDelegateReturnsFalse()
            {
                string directory = this.TestDirectory;
                RepositoryRoot root = RepositoryRoot.Create(directory, Paths.IssueRootFolderName);

                Mock<ILogger> logger = new Mock<ILogger>();
                Mock<IRepository> repository = new Mock<IRepository>();
                Mock<IIssueKeyProvider> keyProvider = new Mock<IIssueKeyProvider>();
                Mock<IIssueConfiguration> configuration = new Mock<IIssueConfiguration>();
                Mock<IChangeLog> changeLog = new Mock<IChangeLog>();
                Mock<ITrackedIssue> tracked = new Mock<ITrackedIssue>();

                IssueKey key = IssueKey.Create("issue-1");
                keyProvider
                    .Setup(provider => provider.TryGetKey("issue-1", out key))
                    .Returns(true);

                bool deletionInvoked = false;
                IssueManager manager = new IssueManager(
                    logger.Object,
                    root,
                    repository.Object,
                    keyProvider.Object,
                    configuration.Object,
                    changeLog.Object,
                    tracked.Object,
                    _ => Mock.Of<IIssue>(),
                    _ =>
                    {
                        deletionInvoked = true;
                        return Task.FromResult(false);
                    },
                    _ => Task.FromResult<IIssue?>(null));

                bool result = await manager.DeleteAsync("issue-1");

                Assert.That(deletionInvoked, Is.True);
                Assert.That(result, Is.False);
            }

            [Test]
            public void DeleteByKey_DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = this.Sut.Create(nameof(Delete.DeleteByKey_DeletesExistingIssue), string.Empty);
                string path = Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(issue.Key));
                Assert.IsTrue(Directory.Exists(path));
                bool result = this.Sut.Delete(issue.Key);
                Assert.That(result, Is.True);
                Assert.IsFalse(Directory.Exists(path));
            }

            [Test]
            public void DeleteByKey_RejectsPathOutsideIssueDirectory()
            {
                this.Initialize(this.TestDirectory);
                string sentinel = Path.Combine(this.TestDirectory, "sentinel.txt");
                File.WriteAllText(sentinel, "preserve");

                Assert.ThrowsAsync<ArgumentException>(async () =>
                    await this.Sut.DeleteAsync(IssueKey.Create("..")));
                Assert.That(File.ReadAllText(sentinel), Is.EqualTo("preserve"));
            }

            [Test]
            public async Task DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Delete.DeletesExistingIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.IsTrue(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(create.Result.Key))));
                SafeResult<bool> delete = await this.Sut
                    .DeleteAsync(create.Result.Key)
                    .WithSafeResultAsync();
                Assert.IsTrue(delete.IsSuccess);
                Assert.IsFalse(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(create.Result.Key))));
            }

            [Test]
            public async Task DeleteLeavesSiblingIssueInTheSameDirectory()
            {
                this.Initialize(this.TestDirectory);
                IIssue first = await this.Manager.CreateAsync("First issue", string.Empty);
                IIssue second = await this.Manager.CreateAsync("Second issue", string.Empty);
                string firstFile = MarkdownIssue.GetMarkdownFile(((Issue)first).Root);
                string secondFile = MarkdownIssue.GetMarkdownFile(((Issue)second).Root);

                Assert.That(await this.Manager.DeleteAsync(first.Key), Is.True);

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(firstFile), Is.False);
                    Assert.That(File.Exists(secondFile), Is.True);
                    Assert.That(Directory.Exists(Path.GetDirectoryName(secondFile)!), Is.True);
                    Assert.That(this.Manager.Find(issue => issue.Key == second.Key).Select(issue => issue.Key), Is.EqualTo(new[] { second.Key }));
                });
            }

            [Test]
            public async Task FailsIfIssueDoesNotExist()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<bool> delete = await this.Sut
                    .DeleteAsync(this.Sut.KeyProvider.Next())
                    .WithSafeResultAsync();
                Assert.IsFalse(delete.IsSuccess);
            }
        }
    }
}
