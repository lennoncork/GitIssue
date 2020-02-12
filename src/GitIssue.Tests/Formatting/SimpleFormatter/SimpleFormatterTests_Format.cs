using System.Threading.Tasks;
using GitIssue.Formatters;
using NUnit.Framework;

namespace GitIssue.Tests.Formatting.SimpleFormatter
{
    [TestFixture]
    public partial class SimpleFormatterTests
    {
        [TestFixture]
        public class Format : SimpleFormatterTests
        {
            [Test]
            public async Task FormatsOutput()
            {
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(FormatsOutput), string.Empty)
                    .WithSafeResultAsync();
                var output = create.Result.Format();
                var expected = $"{create.Result.Key}: {create.Result.Title}";
                Assert.That(output, Is.EqualTo(expected));
            }
        }
    }
}