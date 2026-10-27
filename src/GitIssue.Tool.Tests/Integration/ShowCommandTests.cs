using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        public void Show_PrintsOnlyIssuesAuthoredByCurrentUser()
        {
            this.InitializeRepository();
            const string currentUserTitle = "Issue authored by current user";
            const string otherTitle = "Issue authored by someone else";
            this.CreateIssue(currentUserTitle);
            string otherKey = this.CreateIssue(otherTitle);
            this.SetAuthor(otherKey, "Other Person <other@example.com>");

            int mineExitCode = this.Run("Show", "mine");
            string mineOutput = this.Output;
            string mineError = this.Error;

            int allExitCode = this.Run("Show", "all");

            Assert.Multiple(() =>
            {
                Assert.That(mineExitCode, Is.Zero, mineError);
                Assert.That(mineOutput, Does.Contain("Showing 'my' issues"));
                Assert.That(mineOutput, Does.Contain(currentUserTitle));
                Assert.That(mineOutput, Does.Not.Contain(otherTitle));
                Assert.That(allExitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Showing 'all' issues"));
                Assert.That(this.Output, Does.Contain(currentUserTitle));
                Assert.That(this.Output, Does.Contain(otherTitle));
            });
        }

        private void SetAuthor(string key, string author)
        {
            JsonObject issue = this.ReadIssue(key);
            issue["Author"] = author;
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
            };
            File.WriteAllText(this.GetIssueFile(key), issue.ToJsonString(options));
            this.AssertJsonValue(this.ReadIssue(key), "Author", author);
        }
    }
}