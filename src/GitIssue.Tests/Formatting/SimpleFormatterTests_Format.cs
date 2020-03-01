using GitIssue.Formatters;
using NUnit.Framework;

namespace GitIssue.Tests.Formatting
{
    [TestFixture]
    public partial class SimpleFormatterTests
    {
        [TestFixture]
        public class Format : SimpleFormatterTests
        {
            [Test]
            public void FormatsIssue()
            {
                var issue = Moqs.CreateIssue(nameof(FormatsIssue));
                var output = issue.Format();
                var expected = $"{issue.Key}: {issue.Title}";
                Assert.That(output, Is.EqualTo(expected));
            }
        }
    }
}