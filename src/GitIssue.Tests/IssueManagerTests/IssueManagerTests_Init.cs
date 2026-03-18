using System.IO;
using Autofac.Core;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Init : IssueManagerTests
        {
            [Test]
            public void CreatesConfigFile()
            {
                this.CreatesIssueFolder();
                Assert.IsTrue(File.Exists(this.ConfigFile));
            }

            [Test]
            public void CreatesIssueFolder()
            {
                Repository.Init(this.TestDirectory);
                IssueManager.Init(this.TestDirectory);
                Assert.That(this.TestDirectory, Is.Not.Empty);
                Assert.IsTrue(Directory.Exists(this.IssueDirectory));
                Assert.IsTrue(Directory.Exists(this.GitDirectory));
            }

            [Test]
            public void FailsIfNotAGitRepository()
            {
                Assert.Throws<DependencyResolutionException>(() => { IssueManager.Init(this.TestDirectory); });
            }
        }
    }
}