using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class RemoveCommandTests : IntegrationTestBase
    {
        [Test]
        public void Remove_DeletesValueFromArrayField()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue with removable label");
            Assert.That(this.Run("Add", key, "Labels", "integration"), Is.Zero, this.Error);

            int exitCode = this.Run("Remove", key, "Labels", "integration");

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonArrayDoesNotContain(issue, "Labels", "integration");
            });
        }
    }
}