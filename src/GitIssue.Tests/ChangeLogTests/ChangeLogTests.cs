using System.IO;
using GitIssue.Issues;
using Moq;
using NUnit.Framework;

namespace GitIssue.Tests.ChangeLogTests
{
    [TestFixture]
    public class ChangeLogTests
    {
        [TestFixture]
        public class ReadAndSave : ChangeLogTests
        {
            [Test]
            public void Read_Returns_New_Instance_When_File_Does_Not_Exist()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "changelog.json");

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                ChangeLog log = ChangeLog.Read(file);

                Assert.That(log.Log, Is.Empty);
            }

            [Test]
            public void Save_After_Clear_Persists_Empty_Log()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "changelog.json");

                ChangeLog log = new ChangeLog();
                IssueKey key = IssueKey.Create("ISS-6");

                log.Add(key, ChangeType.Create);
                log.Clear();

                log.Save(file);

                ChangeLog loaded = ChangeLog.Read(file);

                Assert.That(loaded.Log, Is.Empty);
            }

            [Test]
            public void Save_And_Read_Persist_Log_Entries()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "changelog.json");

                ChangeLog log = new ChangeLog();
                IssueKey key = IssueKey.Create("ISS-1");

                log.Add(key, ChangeType.Create, "summary");
                log.Save(file);

                Assert.That(File.Exists(file), Is.True);

                ChangeLog loaded = ChangeLog.Read(file);

                Assert.That(loaded.Log.ContainsKey(key), Is.True);
                Assert.That(loaded.Log[key].Count, Is.EqualTo(1));
            }

            [Test]
            public void Save_Does_Not_Create_File_When_No_Changes()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "changelog.json");

                ChangeLog log = new ChangeLog();

                log.Save(file);

                Assert.That(File.Exists(file), Is.False);
            }
        }

        [TestFixture]
        public class Add : ChangeLogTests
        {
            [Test]
            public void Add_Falls_Back_To_Enum_Name_For_Unknown_ChangeType()
            {
                ChangeLog log = new ChangeLog();
                IssueKey key = IssueKey.Create("ISS-4");
                ChangeType unknown = (ChangeType)999;

                log.Add(key, unknown);

                string entry = log.Log[key][0];

                Assert.That(entry, Does.Contain(unknown.ToString()));
            }

            [Test]
            public void Add_Issue_Uses_Issue_Key()
            {
                ChangeLog log = new ChangeLog();
                IssueKey key = IssueKey.Create("ISS-2");

                Mock<IIssue> issue = new Mock<IIssue>(MockBehavior.Strict);
                issue.Setup(i => i.Key).Returns(key);

                log.Add(issue.Object, ChangeType.Delete);

                Assert.That(log.Log.ContainsKey(key), Is.True);
            }

            [Test]
            public void Add_Uses_DescriptionAttribute_For_Known_ChangeType()
            {
                ChangeLog log = new ChangeLog();
                IssueKey key = IssueKey.Create("ISS-3");

                log.Add(key, ChangeType.Create);

                string entry = log.Log[key][0];

                Assert.That(entry, Does.Contain("Created new issue"));
            }
        }

        [TestFixture]
        public class Clear : ChangeLogTests
        {
            [Test]
            public void Clear_Removes_All_Entries()
            {
                ChangeLog log = new ChangeLog();
                IssueKey key = IssueKey.Create("ISS-5");

                log.Add(key, ChangeType.Create);
                Assert.That(log.Log, Is.Not.Empty);

                log.Clear();

                Assert.That(log.Log, Is.Empty);
            }
        }
    }
}