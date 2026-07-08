using System.IO;
using NUnit.Framework;

namespace GitIssue.Tests
{
    [TestFixture]
    public class RepositoryRootTests
    {
        [TestFixture]
        public class Open : RepositoryRootTests
        {
            [Test]
            public void Open_With_Name_Uses_Issue_Subdirectory()
            {
                string directory = Helpers.CreateTempDirectory();
                const string issuesName = "issues";
                string issuesPath = Path.Combine(directory, issuesName);
                Directory.CreateDirectory(issuesPath);

                RepositoryRoot root = RepositoryRoot.Open(directory, issuesName);

                string fullPath = new DirectoryInfo(directory).FullName;

                Assert.That(root.IsOwnedRepository, Is.False);
                Assert.That(root.RootPath, Is.EqualTo(fullPath));
                Assert.That(root.IssuesPath, Is.EqualTo(issuesPath));
            }

            [Test]
            public void Open_With_Null_Name_Creates_Owned_Root()
            {
                string directory = Helpers.CreateTempDirectory();

                RepositoryRoot root = RepositoryRoot.Open(directory, null);

                string fullPath = new DirectoryInfo(directory).FullName;

                Assert.That(root.IsOwnedRepository, Is.True);
                Assert.That(root.RootPath, Is.EqualTo(fullPath));
                Assert.That(root.IssuesPath, Is.EqualTo(fullPath));
                Assert.That(root.ConfigFile, Is.EqualTo(Path.Combine(root.IssuesPath, Paths.ConfigFileName)));
                Assert.That(root.ChangeLog, Is.EqualTo(Path.Combine(root.IssuesPath, Paths.ChangeLogFileName)));
                Assert.That(root.Tracked, Is.EqualTo(Path.Combine(root.IssuesPath, Paths.TrackedIssueFileName)));
            }
        }
    }
}