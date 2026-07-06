using System;
using System.Collections.Generic;
using GitIssue.Issues;
using Moq;
using NUnit.Framework;

namespace GitIssue.Tests.ChangeLogTests
{
    [TestFixture]
    public class ChangeLogExtensionsTests
    {
        [TestFixture]
        public class GenerateComments : ChangeLogExtensionsTests
        {
            [Test]
            public void Formats_Single_Issue_With_Single_Change()
            {
                IssueKey key = IssueKey.Create("ISS-1");
                Dictionary<IssueKey, List<string>> logData = new Dictionary<IssueKey, List<string>>
                {
                    [key] = new List<string> { "First change" },
                };

                Mock<IChangeLog> changeLog = new Mock<IChangeLog>(MockBehavior.Strict);
                changeLog.SetupGet(l => l.Log).Returns(logData);

                string output = changeLog.Object.GenerateComments();

                string expected = $"Issue: {key}{Environment.NewLine} - First change{Environment.NewLine}";
                Assert.That(output, Is.EqualTo(expected));
            }

            [Test]
            public void Inserts_Blank_Line_Between_Issues()
            {
                IssueKey first = IssueKey.Create("ISS-1");
                IssueKey second = IssueKey.Create("ISS-2");
                Dictionary<IssueKey, List<string>> logData = new Dictionary<IssueKey, List<string>>
                {
                    [first] = new List<string> { "First change" },
                    [second] = new List<string> { "Second change" },
                };

                Mock<IChangeLog> changeLog = new Mock<IChangeLog>(MockBehavior.Strict);
                changeLog.SetupGet(l => l.Log).Returns(logData);

                string output = changeLog.Object.GenerateComments();

                string doubleNewLine = Environment.NewLine + Environment.NewLine;
                Assert.That(output, Does.Contain(doubleNewLine));
                Assert.That(output, Does.Contain($"Issue: {first}"));
                Assert.That(output, Does.Contain($"Issue: {second}"));
            }

            [Test]
            public void Returns_Empty_String_When_Log_Is_Empty()
            {
                Dictionary<IssueKey, List<string>> logData = new Dictionary<IssueKey, List<string>>();
                Mock<IChangeLog> changeLog = new Mock<IChangeLog>(MockBehavior.Strict);
                changeLog.SetupGet(l => l.Log).Returns(logData);

                string output = changeLog.Object.GenerateComments();

                Assert.That(output, Is.EqualTo(string.Empty));
            }
        }
    }
}