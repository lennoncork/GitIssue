using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Find : IssueManagerTests
        {
            [Test]
            public async Task FindsIssueWithoutCapturingSynchronizationContext()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync("Synchronization Context Issue", string.Empty)
                    .WithSafeResultAsync();
                Assert.That(create.IsSuccess, Is.True);

                SynchronizationContext? previous = SynchronizationContext.Current;
                RecordingSynchronizationContext context = new RecordingSynchronizationContext();
                try
                {
                    SynchronizationContext.SetSynchronizationContext(context);

                    IIssue[] issues = this.Sut.Find(issue => issue.Key == create.Result.Key).ToArray();

                    Assert.That(issues, Has.Length.EqualTo(1));
                    Assert.That(context.PostCount, Is.Zero);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(previous);
                }
            }

            [TestCase("New Issue")]
            public async Task FindsIssueByTitle(string title)
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(title, string.Empty)
                    .WithSafeResultAsync();
                IIssue[] find = this.Sut.Find(i => i.Title == title).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Title, Is.EqualTo(title));
            }

            [Test]
            public async Task FindAsync_EnumeratesMatchingIssues()
            {
                this.Initialize(this.TestDirectory);
                string title = "Async Find Issue";
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(title, string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);

                List<IIssue> issues = new List<IIssue>();
                await foreach (IIssue issue in this.Sut.FindAsync(i => i.Title == title))
                {
                    issues.Add(issue);
                }

                Assert.That(issues.Count, Is.EqualTo(1));
                Assert.That(issues[0].Title, Is.EqualTo(title));
            }

            [Test]
            public async Task FindsIssueByKey()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Find.FindsIssueByKey), string.Empty)
                    .WithSafeResultAsync();
                IIssue[] find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Key, Is.EqualTo(create.Result.Key));
            }

            private sealed class RecordingSynchronizationContext : SynchronizationContext
            {
                public int PostCount { get; private set; }

                public override void Post(SendOrPostCallback d, object? state)
                {
                    this.PostCount++;
                    d(state);
                }
            }
        }
    }
}
