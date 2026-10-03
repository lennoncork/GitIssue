using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class AddCommandTests : IntegrationTestBase
    {
        [Test]
        public void Add_AppendsValueToArrayField()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue with label");

            int exitCode = this.Run("Add", key, "Labels", "integration");

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonArrayContains(issue, "Labels", "integration");
            });
        }
    }
}