﻿using System;
using System.Linq;
using System.Threading.Tasks;
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
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(SetsUpdateDate), string.Empty)
                    .WithSafeResultAsync();
                await create.Result.SaveAsync();
                var find = Sut.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That((DateTime)issue.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task UpdatesTitle()
            {
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(UpdatesTitle), string.Empty)
                    .WithSafeResultAsync();
                create.Result.Title = "Updated";
                await create.Result.SaveAsync();
                var find = Sut.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.Title, Is.EqualTo("Updated"));
            }
        }
    }
}