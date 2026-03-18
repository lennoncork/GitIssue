using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Delete : IssueManagerTests
        {
            [Test]
            public async Task DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Delete.DeletesExistingIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.IsTrue(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(create.Result.Key))));
                SafeResult<bool> delete = await this.Sut
                    .DeleteAsync(create.Result.Key)
                    .WithSafeResultAsync();
                Assert.IsTrue(delete.IsSuccess);
                Assert.IsFalse(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(create.Result.Key))));
            }

            [Test]
            public async Task FailsIfIssueDoesNotExist()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<bool> delete = await this.Sut
                    .DeleteAsync(this.Sut.KeyProvider.Next())
                    .WithSafeResultAsync();
                Assert.IsFalse(delete.IsSuccess);
            }
        }
    }
}