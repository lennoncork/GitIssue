using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.GitIssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Delete : GitIssueManagerTests
        {
            [Test]
            public async Task FailsIfIssueDoesNotExist()
            {
                this.Initialize(this.TestDirectory);
                var delete = await this.Sut
                    .DeleteAsync(this.Sut.KeyProvider.Next())
                    .WithSafeResult();
                Assert.IsFalse(delete.IsSuccess);
            }

            [Test]
            public async Task DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(nameof(DeletesExistingIssue), string.Empty)
                    .WithSafeResult();
                Assert.IsTrue(create.IsSuccess);
                Assert.IsTrue(Directory.Exists(Path.Combine(this.IssueDirectory, create.Result.Key.ToString())));
                var delete = await this.Sut
                    .DeleteAsync(create.Result.Key)
                    .WithSafeResult();
                Assert.IsTrue(delete.IsSuccess);
                Assert.IsFalse(Directory.Exists(Path.Combine(this.IssueDirectory, create.Result.Key.ToString())));
            }
        }
    }
}
