using System.IO;
using Autofac.Core;
using GitIssue.Exceptions;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Constructor : IssueManagerTests
        {
            [Test]
            public void FailsIfConfigMissing()
            {
                this.Initialize(this.TestDirectory, true, true, false);
                File.Delete(Path.Combine(this.TestDirectory, Paths.IssueRootFolderName, Paths.ConfigFileName));
                Assert.Throws<DependencyResolutionException>(() => { this.Sut = IssueManager.Open(this.TestDirectory); });
            }

            [Test]
            public void FailsIfDirectoryMissing()
            {
                this.Initialize(this.TestDirectory, true, true, false);
                Directory.Delete(Path.Combine(this.TestDirectory, Paths.IssueRootFolderName), true);
                Assert.Throws<RepositoryNotFoundException>(() => { this.Sut = IssueManager.Open(this.TestDirectory); });
            }

            [Test]
            public void FailsIfGitDirectoryMissing()
            {
                this.Initialize(this.TestDirectory);
                Directory.Delete(Path.Combine(this.TestDirectory, Paths.GitFolderName), true);
                Assert.Throws<DependencyResolutionException>(() =>
                {
                    this.Sut = IssueManager.Open(this.TestDirectory);
                });
            }

            [Test]
            public void SucceedsFromCurrentDirectory()
            {
                using (new Helpers.EnvironmentCurrentDirectory(this.TestDirectory))
                {
                    this.Initialize(this.TestDirectory);
                    Assert.That(this.Sut.Root.RootPath, Is.EqualTo(this.TestDirectory));
                }
            }

            [Test]
            public void SucceedsFromSpecifiedDirectory()
            {
                this.Initialize(this.TestDirectory);
                Assert.That(this.Sut.Root.RootPath, Is.EqualTo(this.TestDirectory));
            }

            [Test]
            public void SucceedsWithDifferentName()
            {
                string name = "Specifications";
                this.Initialize(this.TestDirectory, name);
                Assert.That(this.Sut.Root.RootPath, Is.EqualTo(this.TestDirectory));
                Assert.That(Directory.Exists(Path.Combine(this.TestDirectory, name)), Is.True);
            }
        }
    }
}