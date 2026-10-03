using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class FieldsCommandTests : IntegrationTestBase
    {
        [Test]
        public void Fields_PrintsConfiguredFields()
        {
            this.InitializeRepository();
            string configuration = this.ReadFile(this.ConfigurationFile);

            int exitCode = this.Run("Fields");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("Title: A 'JsonValue' field with 'String' values"));
                Assert.That(this.Output, Does.Contain("Labels: A 'JsonArray' field with 'Label' values"));
                this.AssertFileContents(this.ConfigurationFile, configuration);
                this.AssertFileDoesNotExist(this.TrackingFile);
                this.AssertFileDoesNotExist(this.ChangesFile);
                Assert.That(this.GetIssueKeys(), Is.Empty);
            });
        }
    }
}