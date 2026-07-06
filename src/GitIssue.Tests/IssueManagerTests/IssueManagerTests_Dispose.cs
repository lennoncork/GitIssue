using System.Threading.Tasks;
using GitIssue;
using GitIssue.Issues;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;
using Serilog;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class DisposeTests : IssueManagerTests
        {
            [Test]
            public void Dispose_SavesChangeLogOnce()
            {
                string directory = this.TestDirectory;
                RepositoryRoot root = RepositoryRoot.Create(directory, Paths.IssueRootFolderName);

                Mock<ILogger> logger = new Mock<ILogger>();
                Mock<IRepository> repository = new Mock<IRepository>();
                Mock<IIssueKeyProvider> keyProvider = new Mock<IIssueKeyProvider>();
                Mock<IIssueConfiguration> configuration = new Mock<IIssueConfiguration>();
                Mock<IChangeLog> changeLog = new Mock<IChangeLog>();
                Mock<ITrackedIssue> tracked = new Mock<ITrackedIssue>();

                IssueManager manager = new IssueManager(
                    logger.Object,
                    root,
                    repository.Object,
                    keyProvider.Object,
                    configuration.Object,
                    changeLog.Object,
                    tracked.Object,
                    _ => Mock.Of<IIssue>(),
                    _ => Task.FromResult(true),
                    _ => Task.FromResult<IIssue?>(null));

                manager.Dispose();
                manager.Dispose();

                changeLog.Verify(c => c.Save(root.ChangeLog), Times.Once);
            }

            [Test]
            public async Task DisposeAsync_InvokesDispose()
            {
                string directory = this.TestDirectory;
                RepositoryRoot root = RepositoryRoot.Create(directory, Paths.IssueRootFolderName);

                Mock<ILogger> logger = new Mock<ILogger>();
                Mock<IRepository> repository = new Mock<IRepository>();
                Mock<IIssueKeyProvider> keyProvider = new Mock<IIssueKeyProvider>();
                Mock<IIssueConfiguration> configuration = new Mock<IIssueConfiguration>();
                Mock<IChangeLog> changeLog = new Mock<IChangeLog>();
                Mock<ITrackedIssue> tracked = new Mock<ITrackedIssue>();

                IssueManager manager = new IssueManager(
                    logger.Object,
                    root,
                    repository.Object,
                    keyProvider.Object,
                    configuration.Object,
                    changeLog.Object,
                    tracked.Object,
                    _ => Mock.Of<IIssue>(),
                    _ => Task.FromResult(true),
                    _ => Task.FromResult<IIssue?>(null));

                await manager.DisposeAsync();

                changeLog.Verify(c => c.Save(root.ChangeLog), Times.Once);
            }
        }
    }
}
