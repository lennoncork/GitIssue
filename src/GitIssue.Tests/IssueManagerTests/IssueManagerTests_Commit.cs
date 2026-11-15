using System;
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
                File.WriteAllText(outsideFile, "tracked outside the issues directory");
                Signature signature = new Signature("Tester", "tester@example.com", DateTimeOffset.Now);
                Commands.Stage(this.Sut.Repository, outsideFile);
                this.Sut.Repository.Commit("Track a file outside the issues directory", signature, signature);

                File.WriteAllText(outsideFile, "staged modification");
                Commands.Stage(this.Sut.Repository, outsideFile);
                File.WriteAllText(outsideFile, "unstaged workdir edit");
                Assert.That(this.Sut.Repository.RetrieveStatus().Staged.Any(), Is.True);

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_ReturnsTrueWhenStagedFileIsOutsideIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                Blob committed = (Blob)this.Sut.Repository.Head.Tip["outside.txt"].Target;
                Blob staged = this.Sut.Repository.Lookup<Blob>(this.Sut.Repository.Index["outside.txt"].Id);
                Assert.That(committed.GetContentText(), Is.EqualTo("tracked outside the issues directory"));
                Assert.That(staged.GetContentText(), Is.EqualTo("staged modification"));
                Assert.That(File.ReadAllText(outsideFile), Is.EqualTo("unstaged workdir edit"));
                Assert.That(this.Sut.Repository.Head.Tip[this.findCreatedIssueRelativePath()], Is.Not.Null);
            }

            [Test]
            public async Task CommitAsync_LeavesStagedAdditionOutsideIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                string outsideFile = Path.Combine(this.TestDirectory, "added.txt");
                File.WriteAllText(outsideFile, "staged addition");
                Commands.Stage(this.Sut.Repository, outsideFile);

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_LeavesStagedAdditionOutsideIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                Assert.That(this.Sut.Repository.Head.Tip["added.txt"], Is.Null);
                Blob staged = this.Sut.Repository.Lookup<Blob>(this.Sut.Repository.Index["added.txt"].Id);
                Assert.That(staged.GetContentText(), Is.EqualTo("staged addition"));
                Assert.That(File.ReadAllText(outsideFile), Is.EqualTo("staged addition"));
            }

            [Test]
            public async Task CommitAsync_LeavesStagedDeletionOutsideIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                string outsideFile = Path.Combine(this.TestDirectory, "deleted.txt");
                File.WriteAllText(outsideFile, "still in head");
                Signature signature = new Signature("Tester", "tester@example.com", DateTimeOffset.Now);
                Commands.Stage(this.Sut.Repository, outsideFile);
                this.Sut.Repository.Commit("Track a file that will be deleted", signature, signature);
                File.Delete(outsideFile);
                Commands.Stage(this.Sut.Repository, outsideFile);

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_LeavesStagedDeletionOutsideIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                Assert.That(this.Sut.Repository.Head.Tip["deleted.txt"], Is.Not.Null);
                Assert.That(this.Sut.Repository.Index["deleted.txt"], Is.Null);
                Assert.That(File.Exists(outsideFile), Is.False);
            }

            [Test]
            public async Task CommitAsync_LeavesStagedRenameOutsideIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                string oldFile = Path.Combine(this.TestDirectory, "rename-old.txt");
                File.WriteAllText(oldFile, "renamed content");
                Signature signature = new Signature("Tester", "tester@example.com", DateTimeOffset.Now);
                Commands.Stage(this.Sut.Repository, oldFile);
                this.Sut.Repository.Commit("Track a file that will be renamed", signature, signature);
                Commands.Move(this.Sut.Repository, "rename-old.txt", "rename-new.txt");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_LeavesStagedRenameOutsideIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                Assert.That(this.Sut.Repository.Head.Tip["rename-old.txt"], Is.Not.Null);
                Assert.That(this.Sut.Repository.Head.Tip["rename-new.txt"], Is.Null);
                Assert.That(this.Sut.Repository.Index["rename-old.txt"], Is.Null);
                Blob staged = this.Sut.Repository.Lookup<Blob>(this.Sut.Repository.Index["rename-new.txt"].Id);
                Assert.That(staged.GetContentText(), Is.EqualTo("renamed content"));
            }

            [Test]
            public async Task CommitAsync_ReturnsFalseWhenStagedFileIsInsideIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_ReturnsFalseWhenStagedFileIsInsideIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Commands.Stage(this.Sut.Repository, this.findCreatedIssueRelativePath());

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.False);
                Assert.That(this.Sut.Repository.Info.IsHeadUnborn, Is.True);
            }

            [Test]
            public async Task CommitAsync_DoesNotCommitIgnoredFileUnderIssuesDirectory()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_DoesNotCommitIgnoredFileUnderIssuesDirectory), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                string issueRelativePath = this.findCreatedIssueRelativePath();
                string ignoredRelativePath = Path.GetDirectoryName(issueRelativePath)!.Replace('\\', '/') + "/local-notes.txt";
                File.WriteAllText(Path.Combine(this.TestDirectory, ".gitignore"), ignoredRelativePath + "\n");
                File.WriteAllText(Path.Combine(this.TestDirectory, ignoredRelativePath.Replace('/', Path.DirectorySeparatorChar)), "keep out of the commit");
                Assert.That(this.Sut.Repository.Ignore.IsPathIgnored(ignoredRelativePath), Is.True);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                Assert.That(this.Sut.Repository.Head.Tip[ignoredRelativePath], Is.Null);
                Assert.That(this.Sut.Repository.Index[ignoredRelativePath], Is.Null);
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
                Assert.That(this.Sut.Repository.Head.Tip[Paths.ChangeLogFileName], Is.Null);
                Assert.That(this.Sut.Repository.Head.Tip[Paths.TrackedIssueFileName], Is.Null);
                Assert.That(this.Sut.Repository.Head.Tip[this.findCreatedIssueRelativePath()], Is.Not.Null);
            }

            [Test]
            public async Task CommitAsync_DoesNotCommitStagedDeletionOfChangeLog()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Commit.CommitAsync_DoesNotCommitStagedDeletionOfChangeLog), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                string changeLogRelativePath = Paths.ChangeLogFileName;
                this.Sut.Changes.Save(this.Sut.Root.ChangeLog);
                Signature signature = new Signature("Tester", "tester@example.com", DateTimeOffset.Now);
                Commands.Stage(this.Sut.Repository, changeLogRelativePath);
                this.Sut.Repository.Commit("Track the change log", signature, signature);

                File.Delete(this.Sut.Root.ChangeLog);
                Commands.Stage(this.Sut.Repository, changeLogRelativePath);

                bool result = await this.Sut.CommitAsync();

                Assert.That(result, Is.True);
                TreeEntry committed = this.Sut.Repository.Head.Tip[changeLogRelativePath];
                TreeEntry parent = this.Sut.Repository.Head.Tip.Parents.Single()[changeLogRelativePath];
                Assert.That(committed, Is.Not.Null);
                Assert.That(committed.Target.Id, Is.EqualTo(parent.Target.Id));
                Assert.That(this.Sut.Repository.Index[changeLogRelativePath].Id, Is.EqualTo(committed.Target.Id));
                Assert.That(ChangeLog.Read(this.Sut.Root.ChangeLog).Log, Is.Empty);
            }

            private string findCreatedIssueRelativePath()
            {
                return Directory.EnumerateFiles(this.IssueDirectory, "*", SearchOption.AllDirectories)
                    .Select(path => Path.GetRelativePath(this.TestDirectory, path).Replace('\\', '/'))
                    .Where(path => !path.Equals(".git", StringComparison.Ordinal) && !path.StartsWith(".git/", StringComparison.Ordinal))
                    .Single(path => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                        (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                         !path.Equals(Paths.ChangeLogFileName, StringComparison.Ordinal) &&
                         !path.Equals(Paths.ConfigFileName, StringComparison.Ordinal) &&
                         !path.Equals(Paths.TrackedIssueFileName, StringComparison.Ordinal) &&
                         !path.EndsWith("/" + Paths.ChangeLogFileName, StringComparison.Ordinal) &&
                         !path.EndsWith("/" + Paths.ConfigFileName, StringComparison.Ordinal) &&
                         !path.EndsWith("/" + Paths.TrackedIssueFileName, StringComparison.Ordinal)));
            }
        }
    }
}
