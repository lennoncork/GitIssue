using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class ChangesCommandTests : IntegrationTestBase
    {
        [Test]
        public void Changes_PrintsPendingChangeLog()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Changed issue");

            int exitCode = this.Run("Changes");

            var changes = this.ReadJsonObject(this.ChangesFile);
            var changeLog = changes["Log"]!.AsObject();
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain($"Issue: {key}"));
                Assert.That(this.Output, Does.Contain("Created new issue"));
                this.AssertFileExists(this.ChangesFile);
                this.AssertJsonArrayItemContains(changeLog, key, "Created new issue");
            });
        }
    }
}