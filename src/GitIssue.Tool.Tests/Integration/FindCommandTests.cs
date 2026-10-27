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

            int exitCode = this.Run("Find", "title=Matching issue");

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
        public void Find_PrintsEveryIssueWhenQueryIsEmpty()
        {
            this.InitializeRepository();
            this.CreateIssue("Matching issue", "First description");
            this.CreateIssue("Other issue", "Second description");

            int exitCode = this.Run("Find");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Matching issue"));
                Assert.That(this.Output, Does.Contain("Other issue"));
            });
        }

        [Test]
        public void Find_MatchesDescription()
        {
            this.InitializeRepository();
            this.CreateIssue("Matching issue", "Shared release notes");
            this.CreateIssue("Other issue", "Unrelated work");

            int exitCode = this.Run("Find", "description=Shared release notes");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Matching issue"));
                Assert.That(this.Output, Does.Not.Contain("Other issue"));
            });
        }

        [Test]
        public void Find_MatchesAuthor()
        {
            this.InitializeRepository();
            string matchingKey = this.CreateIssue("Matching issue");
            this.CreateIssue("Other issue");
            string author = this.ReadIssue(matchingKey)["Author"]!.GetValue<string>();

            int exitCode = this.Run("Find", $"author={author}");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Matching issue"));
                Assert.That(this.Output, Does.Contain("Other issue"));
            });
        }

        [Test]
        public void Find_MatchesLabel()
        {
            this.InitializeRepository();
            string matchingKey = this.CreateIssue("Matching issue");
            this.CreateIssue("Other issue");
            this.RunSuccessfully("Add", matchingKey, "Labels", "bug");

            int exitCode = this.Run("Find", "label=bug");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Matching issue"));
                Assert.That(this.Output, Does.Not.Contain("Other issue"));
            });
        }

        [Test]
        public void Find_MatchesTextInTitleOrDescription()
        {
            this.InitializeRepository();
            this.CreateIssue("Matching issue", "Shared release notes");
            this.CreateIssue("Other issue", "Unrelated work");

            int exitCode = this.Run("Find", "text=release notes");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Matching issue"));
                Assert.That(this.Output, Does.Not.Contain("Other issue"));
            });
        }

        [Test]
        public void Find_RejectsMethodCallOrFileRead()
        {
            this.InitializeRepository();
            string matchingKey = this.CreateIssue("Matching issue");
            string otherKey = this.CreateIssue("Other issue");
            string matchingIssue = this.ReadFile(this.GetIssueFile(matchingKey));
            string otherIssue = this.ReadFile(this.GetIssueFile(otherKey));

            int exitCode = this.Run(
                "Find",
                "i => System.IO.File.ReadAllText(\"/etc/passwd\").Length >= 0 || i.Title.ToString().Length >= 0");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(this.Output, Does.Contain("Invalid find predicate"));
                Assert.That(this.Output, Does.Not.Contain("Matching issue"));
                Assert.That(this.Output, Does.Not.Contain("Other issue"));
                this.AssertIssueExists(matchingKey);
                this.AssertIssueExists(otherKey);
                this.AssertFileContents(this.GetIssueFile(matchingKey), matchingIssue);
                this.AssertFileContents(this.GetIssueFile(otherKey), otherIssue);
            });
        }
    }
}
