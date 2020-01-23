using System;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.GitIssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Init : GitIssueManagerTests
        {
            [Test]
            public void CreatesIssueFolder()
            {
                Repository.Init(this.TestDirectory);
                GitIssue.IssueManager.Init(this.TestDirectory);
                Assert.That(this.TestDirectory, Is.Not.Empty);
                Assert.IsTrue(Directory.Exists(this.IssueDirectory));
                Assert.IsTrue(Directory.Exists(this.GitDirectory));
            }

            [Test]
            public void CreatesConfigFile()
            {
                this.CreatesIssueFolder();
                Assert.IsTrue(File.Exists(this.ConfigFile));
            }

            [Test]
            public void FailsIfNotAGitRepository()
            {
                Assert.Throws<RepositoryNotFoundException>(() => { GitIssue.IssueManager.Init(this.TestDirectory); });
            }
        }
    }
}
