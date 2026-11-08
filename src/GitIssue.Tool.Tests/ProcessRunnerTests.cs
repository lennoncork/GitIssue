using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests
{
    [TestFixture]
    public class ProcessRunnerTests
    {
        [Test]
        public void CreateStartInfo_PassesPathWithSpacesAsItsOwnArgument()
        {
            string path = Path.Combine("folder with spaces", "issue file.txt");
            ProcessStartInfo startInfo = ProcessRunner.CreateStartInfo("joe", "-pound_comment", path);

            Assert.Multiple(() =>
            {
                Assert.That(startInfo.FileName, Is.EqualTo("joe"));
                Assert.That(startInfo.UseShellExecute, Is.False);
                Assert.That(startInfo.Arguments, Is.Empty);
                Assert.That(startInfo.ArgumentList, Has.Count.EqualTo(2));
                Assert.That(startInfo.ArgumentList[0], Is.EqualTo("-pound_comment"));
                Assert.That(startInfo.ArgumentList[1], Is.EqualTo(path));
            });
        }
    }
}
