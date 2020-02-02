using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GitIssue.Fields;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    [TestFixture]
    public partial class BugIntegrationTests
    {
        [TestFixture]
        public class FixVersion : BugIntegrationTests
        {
            [Test]
            public async Task CanBeSetFromString()
            { 
                this.Initialize(this.TestDirectory); 
                var create = await this.Issues
                    .CreateAsync(nameof(CanBeSetFromString), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                var fixVersion = new[] {"Blah"};
                create.Result.SetFixVersion(fixVersion);
                await create.Result.SaveAsync();
                var find = this.Issues.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.GetFixVersion(), Is.EqualTo(fixVersion));
            }
        }
    }
}