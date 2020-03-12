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
                CreatesIssueFolder();
                Assert.IsTrue(File.Exists(ConfigFile));
            }

            [Test]
            public void CreatesIssueFolder()
            {
                Repository.Init(TestDirectory);
                GitIssue.IssueManager.Init(TestDirectory);
                Assert.That(TestDirectory, Is.Not.Empty);
                Assert.IsTrue(Directory.Exists(IssueDirectory));
                Assert.IsTrue(Directory.Exists(GitDirectory));
            }

            [Test]
            public void FailsIfNotAGitRepository()
            {
                Assert.Throws<DependencyResolutionException>(() => { GitIssue.IssueManager.Init(TestDirectory); });
            }
        }
    }
}