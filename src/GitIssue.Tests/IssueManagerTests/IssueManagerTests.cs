using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]   
    public partial class IssueManagerTests : TestsBase
    {
        private IssueManager sut;

        public IssueManager Sut
        {
            get
            {
                if (sut == null)
                    return this.Manager;
                return this.sut;
            }
            set { this.sut = value; }
        }
    }
}