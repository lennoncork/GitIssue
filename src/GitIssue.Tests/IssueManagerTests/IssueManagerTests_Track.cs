using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Track : IssueManagerTests
        {
            [Test]
            public async Task TrackAsync_SavesTrackedIssueAndReturnsFalseOnFirstCall()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Track.TrackAsync_SavesTrackedIssueAndReturnsFalseOnFirstCall), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                bool firstResult = await this.Sut.TrackAsync(create.Result.Key);

                Assert.That(firstResult, Is.False);
                Assert.That(System.IO.File.Exists(this.Sut.Root.Tracked), Is.True);

                TrackedIssue tracked = TrackedIssue.Read(this.Sut.Root.Tracked, null);
                Assert.That(tracked.Key, Is.EqualTo(create.Result.Key));
            }

            [Test]
            public async Task Track_ReturnsTrueWhenAlreadyTrackingSameIssue()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Track.Track_ReturnsTrueWhenAlreadyTrackingSameIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                await this.Sut.TrackAsync(create.Result.Key);
                bool secondResult = this.Sut.Track(create.Result.Key);

                Assert.That(secondResult, Is.True);
            }
        }
    }
}
