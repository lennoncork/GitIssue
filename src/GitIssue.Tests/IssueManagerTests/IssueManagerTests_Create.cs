using System;
using System.Threading.Tasks;
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
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(title, description)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.That(create.Result.Title, Is.EqualTo(title));
                Assert.That(create.Result.Description, Is.EqualTo(description));
            }

            [Test]
            public async Task GeneratesUniqueId()
            {
                Initialize(TestDirectory);
                var create1 = await Sut
                    .CreateAsync(nameof(CreatesNewIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create1.IsSuccess);
                var create2 = await Sut
                    .CreateAsync(nameof(CreatesNewIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create2.IsSuccess);
                Assert.That(create1.Result.Key, Is.Not.EqualTo(create2.Result.Key));
            }

            [Test]
            public async Task SetsCreatedDate()
            {
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(SetsCreatedDate), string.Empty)
                    .WithSafeResultAsync();
                Assert.That((DateTime)create.Result.Created, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task SetsUpdatedDate()
            {
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(SetsUpdatedDate), string.Empty)
                    .WithSafeResultAsync();
                Assert.That((DateTime)create.Result.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }
        }
    }
}