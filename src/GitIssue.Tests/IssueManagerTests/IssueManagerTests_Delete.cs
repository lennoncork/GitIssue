﻿using System.IO;
using System.Threading.Tasks;
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
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(DeletesExistingIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.IsTrue(Directory.Exists(Path.Combine(IssueDirectory, Sut.KeyProvider.GetIssuePath(create.Result.Key))));
                var delete = await Sut
                    .DeleteAsync(create.Result.Key)
                    .WithSafeResultAsync();
                Assert.IsTrue(delete.IsSuccess);
                Assert.IsFalse(Directory.Exists(Path.Combine(IssueDirectory, Sut.KeyProvider.GetIssuePath(create.Result.Key))));
            }

            [Test]
            public async Task FailsIfIssueDoesNotExist()
            {
                Initialize(TestDirectory);
                var delete = await Sut
                    .DeleteAsync(Sut.KeyProvider.Next())
                    .WithSafeResultAsync();
                Assert.IsFalse(delete.IsSuccess);
            }
        }
    }
}