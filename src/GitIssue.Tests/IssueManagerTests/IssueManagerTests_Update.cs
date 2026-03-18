using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Update : IssueManagerTests
        {
            [Test]
            public async Task SetsUpdateDate()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Update.SetsUpdateDate), string.Empty)
                    .WithSafeResultAsync();
                await create.Result.SaveAsync();
                IIssue[] find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                IIssue issue = find[0];
                Assert.That((DateTime)issue.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task UpdatesTitle()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Update.UpdatesTitle), string.Empty)
                    .WithSafeResultAsync();
                create.Result.Title = "Updated";
                await create.Result.SaveAsync();
                IIssue[] find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                IIssue issue = find[0];
                Assert.That(issue.Title, Is.EqualTo("Updated"));
            }
        }
    }
}