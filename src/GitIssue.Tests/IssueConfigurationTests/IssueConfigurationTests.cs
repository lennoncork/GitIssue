using System.IO;
using GitIssue.Fields;
using GitIssue.Issues.File;
using NUnit.Framework;

namespace GitIssue.Tests.IssueConfigurationTests
{
    [TestFixture]
    public class IssueConfigurationTests
    {
        [TestFixture]
        public class Defaults : IssueConfigurationTests
        {
            [Test]
            public void DefaultConfiguration_Has_Expected_Default_Fields()
            {
                IssueConfiguration config = new IssueConfiguration();

                Assert.That(config.KeyProvider.Type, Is.EqualTo(typeof(FileIssueKeyProvider)));
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Key")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Title")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Description")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Author")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Created")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Updated")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Labels")), Is.True);
                Assert.That(config.Fields.ContainsKey(FieldKey.Create("Comments")), Is.True);
            }
        }

        [TestFixture]
        public class SaveAndRead : IssueConfigurationTests
        {
            [Test]
            public void Save_And_Read_RoundTrips_Configuration()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");

                IssueConfiguration config = new IssueConfiguration();

                config.Save(file);

                Assert.That(File.Exists(file), Is.True);

                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(loaded.Fields.Keys, Is.EquivalentTo(config.Fields.Keys));
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(config.KeyProvider.Type));
            }
        }
    }
}