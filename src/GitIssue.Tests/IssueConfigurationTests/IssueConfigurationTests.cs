using System;
using System.IO;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.File;
using GitIssue.Issues.Json;
using GitIssue.Values;
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

                Assert.That(config.KeyProvider.Type, Is.EqualTo(typeof(DateIssueKeyProvider)));
                Assert.That(config.IssueType, Is.EqualTo("ISSUE"));
                Assert.That(config.KeyTemplate, Is.EqualTo(IssueKeyTemplate.Date));
                Assert.That(config.Storage, Is.EqualTo(IssueStorage.Markdown));
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
            [TestCase("bug fix")]
            [TestCase("bug_fix")]
            [TestCase("")]
            public void Save_RejectsInvalidIssueType(string type)
            {
                string file = Path.Combine(Helpers.CreateTempDirectory(), "config.json");
                IssueConfiguration config = new IssueConfiguration { IssueType = type };

                Assert.That(() => config.Save(file), Throws.TypeOf<AggregateException>());
                Assert.That(File.Exists(file), Is.False);
            }

            [Test]
            public void Read_RejectsInvalidIssueType()
            {
                string file = Path.Combine(Helpers.CreateTempDirectory(), "config.json");
                File.WriteAllText(file, "{\"IssueType\":\"bug fix\"}");

                AggregateException? error = Assert.Throws<AggregateException>(() => IssueConfiguration.Read(file));
                Assert.That(error!.InnerException, Is.TypeOf<ArgumentException>());
            }

            [Test]
            public void Save_And_Read_RoundTrips_Configuration()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");

                IssueConfiguration config = new IssueConfiguration();

                config.Save(file);

                Assert.That(File.Exists(file), Is.True);
                string json = File.ReadAllText(file);
                Assert.That(json, Does.Not.Contain(nameof(FieldInfo.FieldMetadata)));
                Assert.That(json, Does.Not.Contain(nameof(FieldInfo.ValueMetadata)));

                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(loaded.Fields.Keys, Is.EquivalentTo(config.Fields.Keys));
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(config.KeyProvider.Type));
                Assert.That(loaded.Storage, Is.EqualTo(IssueStorage.Markdown));
                Assert.That(json, Does.Contain("\"Storage\": \"Markdown\""));
                Assert.That(json, Does.Contain("\"KeyProvider\": \"DateIssueKeyProvider\""));
                Assert.That(json, Does.Contain("\"IssueType\": \"ISSUE\""));
                Assert.That(json, Does.Contain("\"KeyTemplate\":"));
            }

            [Test]
            public void Save_And_Read_RoundTrips_KeyTemplate()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                IssueConfiguration config = new IssueConfiguration
                {
                    KeyProvider = TypeValue.Create<TemplateIssueKeyProvider>(),
                    IssueType = "FEAT",
                    KeyTemplate = "{type}-{uid}-{slug}",
                };

                config.Save(file);
                string json = File.ReadAllText(file);
                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(json, Does.Contain("\"KeyTemplate\": \"{type}-{uid}-{slug}\""));
                Assert.That(json, Does.Contain("\"IssueType\": \"FEAT\""));
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(typeof(TemplateIssueKeyProvider)));
                Assert.That(loaded.IssueType, Is.EqualTo("FEAT"));
                Assert.That(loaded.KeyTemplate, Is.EqualTo("{type}-{uid}-{slug}"));
            }

            [Test]
            public void Save_And_Read_RoundTrips_NameIssueKeyProvider()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                IssueConfiguration config = new IssueConfiguration
                {
                    KeyProvider = TypeValue.Create<NameIssueKeyProvider>(),
                };

                config.Save(file);
                string json = File.ReadAllText(file);
                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(json, Does.Contain("\"KeyProvider\": \"NameIssueKeyProvider\""));
                Assert.That(json, Does.Contain($"\"KeyTemplate\": \"{IssueKeyTemplate.Name}\""));
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(typeof(NameIssueKeyProvider)));
                Assert.That(loaded.KeyTemplate, Is.EqualTo(IssueKeyTemplate.Name));
            }

            [Test]
            public void Save_WritesFriendlyFieldTypeNames()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                IssueConfiguration config = new IssueConfiguration();

                config.Save(file);
                string json = File.ReadAllText(file);
                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(json, Does.Contain("\"FieldType\": \"ValueField\""));
                Assert.That(json, Does.Contain("\"FieldType\": \"ArrayField\""));
                Assert.That(json, Does.Not.Contain("JsonValue"));
                Assert.That(json, Does.Not.Contain("JsonArray"));
                Assert.That(loaded.Fields[FieldKey.Create("Title")].FieldType.ToString(), Is.EqualTo("ValueField"));
                Assert.That(loaded.Fields[FieldKey.Create("Labels")].FieldType.ToString(), Is.EqualTo("ArrayField"));
                Assert.That(loaded.Fields[FieldKey.Create("Title")].FieldType.Type, Is.EqualTo(typeof(JsonValueField)));
                Assert.That(loaded.Fields[FieldKey.Create("Title")].ValueType.Type, Is.EqualTo(typeof(GitIssue.Values.String)));
                Assert.That(loaded.Fields[FieldKey.Create("Labels")].FieldType.Type, Is.EqualTo(typeof(JsonArrayField)));
                Assert.That(loaded.Fields[FieldKey.Create("Labels")].ValueType.Type, Is.EqualTo(typeof(Label)));
            }

            [Test]
            public void Read_AcceptsLegacyJsonFieldTypeNames()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                File.WriteAllText(file, """
                    {
                      "Fields": {
                        "Title": {
                          "FieldType": "JsonValue",
                          "ValueType": "String",
                          "Required": true
                        },
                        "Labels": {
                          "FieldType": "JsonArray",
                          "ValueType": "Label"
                        }
                      }
                    }
                    """);

                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(loaded.Fields[FieldKey.Create("Title")].FieldType.Type, Is.EqualTo(typeof(JsonValueField)));
                Assert.That(loaded.Fields[FieldKey.Create("Title")].ValueType.Type, Is.EqualTo(typeof(GitIssue.Values.String)));
                Assert.That(loaded.Fields[FieldKey.Create("Labels")].FieldType.Type, Is.EqualTo(typeof(JsonArrayField)));
                Assert.That(loaded.Fields[FieldKey.Create("Labels")].ValueType.Type, Is.EqualTo(typeof(Label)));
            }

            [Test]
            public void Read_ResolvesLegacyFileIssueKeyProvider()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                File.WriteAllText(file, """
                    {
                      "KeyProvider": "GitIssue.Issues.File.FileIssueKeyProvider",
                      "Storage": "Json",
                      "Fields": {}
                    }
                    """);

                IssueConfiguration loaded = IssueConfiguration.Read(file);

#pragma warning disable CS0618
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(typeof(FileIssueKeyProvider)));
#pragma warning restore CS0618
            }

            [Test]
            public void Read_TreatsOmittedStorageAsJson()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                IssueConfiguration config = new IssueConfiguration
                {
                    Storage = IssueStorage.Json,
                };

                config.Save(file);
                string json = File.ReadAllText(file);
                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(json, Does.Not.Contain("\"Storage\""));
                Assert.That(loaded.Storage, Is.EqualTo(IssueStorage.Json));
            }
        }
    }
}
