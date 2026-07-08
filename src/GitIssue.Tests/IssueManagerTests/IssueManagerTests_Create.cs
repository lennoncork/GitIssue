using System;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Create : IssueManagerTests
        {
            [TestCase("New Issue", "This Is A New Issue")]
            public async Task CreatesNewIssue(string title, string description)
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(title, description)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.That(create.Result.Title, Is.EqualTo(title));
                Assert.That(create.Result.Description, Is.EqualTo(description));
            }

            [Test]
            public async Task GeneratesUniqueId()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create1 = await this.Sut
                    .CreateAsync(nameof(Create.CreatesNewIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create1.IsSuccess);
                SafeResult<IIssue> create2 = await this.Sut
                    .CreateAsync(nameof(Create.CreatesNewIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create2.IsSuccess);
                Assert.That(create1.Result.Key, Is.Not.EqualTo(create2.Result.Key));
            }

            [Test]
            public async Task SetsCreatedDate()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Create.SetsCreatedDate), string.Empty)
                    .WithSafeResultAsync();
                Assert.That((DateTime)create.Result.Created, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task SetsUpdatedDate()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Create.SetsUpdatedDate), string.Empty)
                    .WithSafeResultAsync();
                Assert.That((DateTime)create.Result.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public void CreatesNewIssueUsingSyncOverload()
            {
                this.Initialize(this.TestDirectory);
                string title = "New Sync Issue";
                IIssue issue = this.Sut.Create(title);
                Assert.That(issue.Title, Is.EqualTo(title));
                Assert.That(issue.Key, Is.Not.EqualTo(IssueKey.None));
            }

            [Test]
            public void CreatesNewIssueUsingSyncOverloadWithDescription()
            {
                this.Initialize(this.TestDirectory);
                string title = "New Sync Issue With Description";
                string description = "This Is A New Sync Issue";
                IIssue issue = this.Sut.Create(title, description);
                Assert.That(issue.Title, Is.EqualTo(title));
                Assert.That(issue.Description, Is.EqualTo(description));
            }
        }
    }
}