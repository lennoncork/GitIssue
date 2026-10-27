using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class FindCommandTests : IntegrationTestBase
    {
        [Test]
        public void Find_PrintsMatchingIssue()
        {
            this.InitializeRepository();
            string matchingKey = this.CreateIssue("Matching issue");
            string otherKey = this.CreateIssue("Other issue");
            string matchingIssue = this.ReadFile(this.GetIssueFile(matchingKey));
            string otherIssue = this.ReadFile(this.GetIssueFile(otherKey));

            int exitCode = this.Run("Find", "i => i.Title.ToString() == \"Matching issue\"");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Matching issue"));
                Assert.That(this.Output, Does.Not.Contain("Other issue"));
                this.AssertIssueExists(matchingKey);
                this.AssertIssueExists(otherKey);
                this.AssertFileContents(this.GetIssueFile(matchingKey), matchingIssue);
                this.AssertFileContents(this.GetIssueFile(otherKey), otherIssue);
            });
        }

        [Test]
        public void Find_ReturnsZeroWhenNothingMatches()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Unmatched issue");

            int exitCode = this.Run("Find", "i => i.Title.ToString() == \"Missing issue\"");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Not.Contain("Unmatched issue"));
                this.AssertIssueExists(key);
            });
        }
    }
}