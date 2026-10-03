using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class TrackCommandTests : IntegrationTestBase
    {
        [Test]
        public void Track_PersistsTrackedIssue()
        {
            this.InitializeRepository();
            string firstKey = this.CreateIssue("First issue");
            string key = this.CreateIssue("Issue to track");
            var initialTracking = this.ReadJsonObject(this.TrackingFile);
            this.AssertJsonValue(initialTracking, "Key", firstKey);

            int exitCode = this.Run("Track", key);

            var tracking = this.ReadJsonObject(this.TrackingFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertFileExists(this.TrackingFile);
                this.AssertJsonValue(tracking, "Key", key);
            });
        }
    }
}