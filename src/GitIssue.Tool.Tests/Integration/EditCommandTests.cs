using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class EditCommandTests : IntegrationTestBase
    {
        [Test]
        public void Edit_UpdatesIssueField()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title");

            int exitCode = this.Run("Edit", key, "Title", "Updated title");

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Title", "Updated title");
            });
        }
    }
}