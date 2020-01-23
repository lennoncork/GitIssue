using System;
using NUnit.Framework;

namespace GitIssue.Tests.IssueKeyProvider
{
    [TestFixture]
    public partial class IssueKeyProviderTests
    {
        [TestFixture]
        public class Next : IssueKeyProviderTests
        {
            [Test]
            public void IncrementsKeyCount()
            {
                int index = new Random().Next(1000);
                //this.Sut.Index = index;
                IssueKey key = this.Sut.Next();
                //Assert.That(key);
            }
        }
    }
}
