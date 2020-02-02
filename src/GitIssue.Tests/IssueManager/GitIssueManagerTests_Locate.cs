﻿using System;
using System.IO;
using GitIssue.Exceptions;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Constructor : GitIssueManagerTests
        {
            [Test]
            public void FailsIfConfigMissing()
            {
                Initialize(TestDirectory, true, true, false);
                File.Delete(Path.Combine(TestDirectory, Paths.IssueRootFolderName, Paths.ConfigFileName));
                Assert.Throws<AggregateException>(() => { Sut = new GitIssue.IssueManager(TestDirectory); });
            }

            [Test]
            public void FailsIfDirectoryMissing()
            {
                Initialize(TestDirectory, true, true, false);
                Directory.Delete(Path.Combine(TestDirectory, Paths.IssueRootFolderName), true);
                Assert.Throws<RepositoryNotFoundException>(() => { Sut = new GitIssue.IssueManager(TestDirectory); });
            }

            [Test]
            public void FailsIfGitDirectoryMissing()
            {
                Initialize(TestDirectory);
                Directory.Delete(Path.Combine(TestDirectory, Paths.GitFolderName), true);
                Assert.Throws<LibGit2Sharp.RepositoryNotFoundException>(() =>
                {
                    Sut = new GitIssue.IssueManager(TestDirectory);
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