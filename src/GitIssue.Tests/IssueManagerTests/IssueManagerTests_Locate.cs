using System;
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
                Initialize(TestDirectory, true, true, false);
                File.Delete(Path.Combine(TestDirectory, Paths.IssueRootFolderName, Paths.ConfigFileName));
                Assert.Throws<DependencyResolutionException>(() => { Sut = IssueManager.Open(TestDirectory); });
            }

            [Test]
            public void FailsIfDirectoryMissing()
            {
                Initialize(TestDirectory, true, true, false);
                Directory.Delete(Path.Combine(TestDirectory, Paths.IssueRootFolderName), true);
                Assert.Throws<RepositoryNotFoundException>(() => { Sut = IssueManager.Open(TestDirectory); });
            }

            [Test]
            public void FailsIfGitDirectoryMissing()
            {
                Initialize(TestDirectory);
                Directory.Delete(Path.Combine(TestDirectory, Paths.GitFolderName), true);
                Assert.Throws<DependencyResolutionException>(() =>
                {
                    Sut = IssueManager.Open(TestDirectory);
                });
            }

            [Test]
            public void SucceedsFromCurrentDirectory()
            {
                using (new Helpers.EnvironmentCurrentDirectory(TestDirectory))
                {
                    Initialize(TestDirectory);
                    Assert.That(Sut.Root.RootPath, Is.EqualTo(TestDirectory));
                }
            }

            [Test]
            public void SucceedsFromSpecifiedDirectory()
            {
                Initialize(TestDirectory);
                Assert.That(Sut.Root.RootPath, Is.EqualTo(TestDirectory));
            }

            [Test]
            public void SucceedsWithDifferentName()
            {
                var name = "Specifications";
                Initialize(TestDirectory, name);
                Assert.That(Sut.Root.RootPath, Is.EqualTo(TestDirectory));
                Assert.That(Directory.Exists(Path.Combine(TestDirectory, name)), Is.True);
            }
        }
    }
}