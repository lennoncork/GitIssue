using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class ShowCommandTests : IntegrationTestBase
    {
        [Test]
        public void Show_PrintsAllIssues()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue to show");
            string issue = this.ReadFile(this.GetIssueFile(key));

            int exitCode = this.Run("Show", "all");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Showing 'all' issues"));
                Assert.That(this.Output, Does.Contain("Issue to show"));
                this.AssertIssueExists(key);
                this.AssertFileContents(this.GetIssueFile(key), issue);
            });
        }

        [Test]
        public void Show_ReturnsZeroWhenNoIssuesExist()
        {
            this.InitializeRepository();

            int exitCode = this.Run("Show", "all");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("No issues to show"));
            });
        }
    }
}