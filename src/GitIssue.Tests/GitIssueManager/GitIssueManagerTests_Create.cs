using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitIssue.Tests.GitIssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Create : GitIssueManagerTests
        {
            [TestCase("New Issue", "This Is A New Issue")]
            public async Task CreatesNewIssue(string title, string description)
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(title, description)
                    .WithSafeResult();
                Assert.IsTrue(create.IsSuccess);
                Assert.That(create.Result.Title, Is.EqualTo(title));
                Assert.That(create.Result.Description, Is.EqualTo(description));
            }

            [Test]
            public async Task GeneratesUniqueId()
            {
                this.Initialize(this.TestDirectory);
                var create1 = await this.Sut
                    .CreateAsync(nameof(CreatesNewIssue), string.Empty)
                    .WithSafeResult();
                Assert.IsTrue(create1.IsSuccess);
                var create2 = await this.Sut
                    .CreateAsync(nameof(CreatesNewIssue), string.Empty)
                    .WithSafeResult();
                Assert.IsTrue(create2.IsSuccess);
                Assert.That(create1.Result.Key, Is.Not.EqualTo(create2.Result.Key));
            }

            [Test]
            public async Task SetsCreatedDate()
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(nameof(SetsCreatedDate), string.Empty)
                    .WithSafeResult();
                Assert.That(create.Result.Created, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task SetsUpdatedDate()
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(nameof(SetsUpdatedDate), string.Empty)
                    .WithSafeResult();
                Assert.That(create.Result.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task CreatesCommit()
            {
                throw new NotImplementedException();
            }
        }
    }
}
