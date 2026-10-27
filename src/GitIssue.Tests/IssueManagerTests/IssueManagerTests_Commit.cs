using System.IO;
using System.Linq;
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
            public async Task CommitAsync_ReturnsTrueWhenStagedFileIsOutsideIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                string outsideFile = Path.Combine(this.TestDirectory, "outside.txt");
                File.WriteAllText(outsideFile, "staged outside the issues directory");
                this.Sut.Repository.Index.Add("outside.txt");
                this.Sut.Repository.Index.Write();
                Assert.That(this.Sut.Repository.RetrieveStatus().Staged.Any(), Is.True);

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_ReturnsTrueWhenStagedFileIsOutsideIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
            }

            [Test]
            public async Task CommitAsync_DoesNotCommitIgnoredFileUnderIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                string ignoredRelativePath = $"{Paths.IssueRootFolderName}/local-notes.txt";
                File.WriteAllText(Path.Combine(this.TestDirectory, ".gitignore"), ignoredRelativePath + "\n");
                File.WriteAllText(Path.Combine(this.IssueDirectory, "local-notes.txt"), "keep out of the commit");
                Assert.That(this.Sut.Repository.Ignore.IsPathIgnored(ignoredRelativePath), Is.True);

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_DoesNotCommitIgnoredFileUnderIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                Assert.That(this.Sut.Repository.Head.Tip[ignoredRelativePath], Is.Null);
            }

            [Test]
            public async Task CommitAsync_SavesClearedChangeLogBeforeReturning()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_SavesClearedChangeLogBeforeReturning), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                this.Sut.Changes.Save(this.Sut.Root.ChangeLog);
                ChangeLog before = ChangeLog.Read(this.Sut.Root.ChangeLog);
                Assert.That(before.Log.ContainsKey(create.Result.Key), Is.True);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                ChangeLog after = ChangeLog.Read(this.Sut.Root.ChangeLog);
                Assert.That(after.Log.ContainsKey(create.Result.Key), Is.False);
                Assert.That(after.Log, Is.Empty);
            }
        }
    }
}