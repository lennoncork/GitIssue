using GitIssue.Formatters;
using GitIssue.Issues;
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
                IReadOnlyIssue issue = Moqs.CreateIssue(nameof(Format.FormatsIssue));
                string output = issue.Format();
                string expected = $"{issue.Key}: {issue.Title}";
                Assert.That(output, Is.EqualTo(expected));
            }
        }
    }
}