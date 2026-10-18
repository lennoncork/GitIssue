using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class InitCommandTests : IntegrationTestBase
    {
        [Test]
        public void Init_CreatesIssueRepository()
        {
            this.AssertDirectoryDoesNotExist(this.IssuesDirectory);

            int exitCode = this.Run("Init");

            var configuration = this.ReadJsonObject(this.ConfigurationFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertDirectoryExists(this.IssuesDirectory);
                this.AssertFileExists(this.ConfigurationFile);
                this.AssertFileDoesNotExist(this.TrackingFile);
                this.AssertFileDoesNotExist(this.ChangesFile);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                this.AssertJsonPropertyExists(configuration, "Fields");
                this.AssertJsonPropertyExists(configuration["Fields"]!.AsObject(), "Title");
                Assert.That(this.Output, Does.Contain("Initialized empty 'Issue' repository"));
            });
        }
    }
}