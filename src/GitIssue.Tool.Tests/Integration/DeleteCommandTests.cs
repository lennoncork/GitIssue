using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class DeleteCommandTests : IntegrationTestBase
    {
        [Test]
        public void Delete_RemovesExistingIssue()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue to delete");
            this.AssertIssueExists(key);

            int exitCode = this.Run("Delete", key);

            var tracking = this.ReadJsonObject(this.TrackingFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueDoesNotExist(key);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                this.AssertFileExists(this.TrackingFile);
                this.AssertJsonValue(tracking, "Key", "None");
                Assert.That(this.Output, Does.Contain($"Deleted issue '{key}'"));
            });
        }
    }
}