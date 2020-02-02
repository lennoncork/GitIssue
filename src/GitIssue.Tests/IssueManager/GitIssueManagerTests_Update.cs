using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Update : GitIssueManagerTests
        {
            [Test]
            public async Task UpdatesTitle()
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(nameof(UpdatesTitle), string.Empty)
                    .WithSafeResultAsync();
                create.Result.Title = "Updated";
                await create.Result.SaveAsync();
                var find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.Title, Is.EqualTo("Updated"));
            }

            [Test]
            public async Task SetsUpdateDate()
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(nameof(SetsUpdateDate), string.Empty)
                    .WithSafeResultAsync();
                await create.Result.SaveAsync();
                var find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }
        }
    }
}
