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

        [Test]
        public void Edit_ReturnsNonZeroWhenIssueIsNotFound()
        {
            this.InitializeRepository();

            int exitCode = this.Run("Edit", "MISSING-1", "Title", "Updated title");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(this.Output + this.Error, Does.Contain("not found"));
                Assert.That(this.GetIssueKeys(), Is.Empty);
            });
        }
    }
}