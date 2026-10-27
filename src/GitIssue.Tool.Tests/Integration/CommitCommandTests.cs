using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class CommitCommandTests : IntegrationTestBase
    {
        [Test]
        public void Commit_CreatesGitCommit()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue to commit");

            int exitCode = this.Run("Commit");

            var changes = this.ReadJsonObject(this.ChangesFile);
            using Repository repository = new Repository(this.TestDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(repository.Head.Tip, Is.Not.Null);
                this.AssertFileExists(this.ConfigurationFile);
                this.AssertIssueExists(key);
                Assert.That(repository.Head.Tip!.Message, Does.Contain(key));
                Assert.That(repository.Head.Tip.Tree[".issues"], Is.Not.Null);
                Assert.That(changes["Log"]!.AsObject(), Is.Empty);
                Assert.That(this.Output, Does.Contain("Committed changes"));
            });
        }

        [Test]
        public void Commit_ReturnsNonZeroWhenIndexAlreadyHasStagedChanges()
        {
            this.InitializeRepository();
            string stagedFile = Path.Combine(this.TestDirectory, "staged.txt");
            File.WriteAllText(stagedFile, "staged");
            using (Repository repository = new Repository(this.TestDirectory))
            {
                LibGit2Sharp.Commands.Stage(repository, stagedFile);
            }

            int exitCode = this.Run("Commit");

            using Repository after = new Repository(this.TestDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(after.Head.Tip, Is.Null);
                Assert.That(this.Output, Does.Not.Contain("Committed changes"));
            });
        }
    }
}