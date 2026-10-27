using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class CommentCommandTests : IntegrationTestBase
    {
        [Test]
        public void Comment_AppendsCommentToIssue()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue with comment");

            int exitCode = this.Run("Comment", key, "Integration comment");

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonArrayContains(issue, "Comments", "Integration comment");
                this.AssertJsonArrayCount(issue, "Comments", 1);
            });
        }
    }
}