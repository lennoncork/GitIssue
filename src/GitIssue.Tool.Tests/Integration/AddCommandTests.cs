using System.IO;
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

        [Test]
        public void Add_RejectsMissingIssue()
        {
            this.InitializeRepository();

            int exitCode = this.Run("Add", "missing", "Labels", "integration");

            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("not found"));
        }

        [Test]
        public void Add_RejectsMissingField()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue with label");

            int exitCode = this.Run("Add", key);

            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("Field must be supplied to add"));
        }

        [TestCase("Unknown", "integration", "does not exist")]
        [TestCase("Title", "integration", "cannot add value")]
        [TestCase("Labels", "two words", "not a valid value")]
        public void Add_RejectsInvalidFieldOrValue(string field, string value, string expected)
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue with label");
            string original = File.ReadAllText(this.GetIssueFile(key));

            int exitCode = this.Run("Add", key, field, value);

            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain(expected));
            Assert.That(File.ReadAllText(this.GetIssueFile(key)), Is.EqualTo(original));
        }

        [Test]
        public void Add_RejectsDuplicateValueWithoutChangingIssue()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue with label");
            this.RunSuccessfully("Add", key, "Labels", "integration");
            string original = File.ReadAllText(this.GetIssueFile(key));

            int exitCode = this.Run("Add", key, "Labels", "integration");

            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("already exists"));
            Assert.That(File.ReadAllText(this.GetIssueFile(key)), Is.EqualTo(original));
        }
    }
}
