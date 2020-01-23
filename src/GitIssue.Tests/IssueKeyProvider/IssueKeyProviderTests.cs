using NUnit.Framework;

namespace GitIssue.Tests.IssueKeyProvider
{
    using SUT = Providers.IssueKeyProvider;

    [TestFixture]
    public partial class IssueKeyProviderTests
    {
        
        protected SUT Sut;

        public IssueKeyProviderTests()
        {
            
        }

        [SetUp]
        public void Setup()
        {
            //this.Sut = new SUT();
        }
    }
}
