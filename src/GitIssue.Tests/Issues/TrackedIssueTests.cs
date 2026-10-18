using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class TrackedIssueTests
    {
        [Test]
        public async Task SaveAndRead_RoundTripsTrackedIssue()
        {
            string file = Path.Combine(Helpers.CreateTempDirectory(), "tracking.json");
            TrackedIssue expected = new TrackedIssue(IssueKey.Create("ISS-TRACKED-1"));

            await expected.SaveAsync(file);
            TrackedIssue actual = TrackedIssue.Read(file);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(file), Is.True);
                Assert.That(actual.Key, Is.EqualTo(expected.Key));
                Assert.That(actual.Started, Is.EqualTo(expected.Started));
                Assert.That(actual, Is.EqualTo(expected));
            });
        }

        [Test]
        public void Read_ReturnsNoneForMissingOrInvalidFile()
        {
            string file = Path.Combine(Helpers.CreateTempDirectory(), "tracking.json");

            TrackedIssue missing = TrackedIssue.Read(file);
            File.WriteAllText(file, "not json");
            TrackedIssue invalid = TrackedIssue.Read(file);

            Assert.Multiple(() =>
            {
                Assert.That(missing, Is.EqualTo(TrackedIssue.None));
                Assert.That(invalid, Is.EqualTo(TrackedIssue.None));
                Assert.That(missing != new TrackedIssue(IssueKey.Create("ISS-TRACKED-1")), Is.True);
            });
        }
    }
}