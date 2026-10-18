using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class CreateCommandTests : IntegrationTestBase
    {
        [Test]
        public void Create_PersistsNewIssue()
        {
            this.InitializeRepository();

            int exitCode = this.Run("Create", "Integration title", "Integration description");

            string[] keys = this.GetIssueKeys();
            Assert.That(keys, Has.Length.EqualTo(1));
            string key = keys[0];
            var issue = this.ReadIssue(key);
            var tracking = this.ReadJsonObject(this.TrackingFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Key", key);
                this.AssertJsonValue(issue, "Title", "Integration title");
                this.AssertJsonValue(issue, "Description", "Integration description");
                this.AssertFileExists(this.TrackingFile);
                this.AssertJsonValue(tracking, "Key", key);
                Assert.That(this.Output, Does.Contain("Created and tracking new issue"));
            });
        }
    }
}