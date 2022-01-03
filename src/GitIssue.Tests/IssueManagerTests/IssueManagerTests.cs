using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests : TestsBase
    {
        private IIssueManager? sut;

        public IIssueManager Sut
        {
            get
            {
                if (sut == null)
                    return this.Manager;
                return this.sut;
            }
            set => this.sut = value;
        }
    }
}