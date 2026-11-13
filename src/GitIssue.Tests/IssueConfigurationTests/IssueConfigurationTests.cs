using System;
using System.Collections.Generic;
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
                Assert.That(config.IssueType, Is.EqualTo("Issue"));
                Assert.That(config.KeyTemplate, Is.EqualTo(IssueKeyTemplate.Default));
                Assert.That(config.Storage, Is.EqualTo(IssueStorage.Markdown));
                Assert.That(config.Issues, Has.Count.EqualTo(1));
                Assert.That(config.Issues[0].Name, Is.EqualTo("Issue"));
                Dictionary<FieldKey, FieldInfo> fields = config.GetSchema(config.IssueType);
                Assert.That(fields.ContainsKey(FieldKey.Create("Key")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Title")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Description")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Author")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Created")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Updated")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Labels")), Is.True);
                Assert.That(fields.ContainsKey(FieldKey.Create("Comments")), Is.True);
            }
        }

        [TestFixture]
        public class SaveAndRead : IssueConfigurationTests
        {
            [Test]
            public void Save_RejectsMissingCoreFieldsAndDuplicateTypes()
            {
                string file = Path.Combine(Helpers.CreateTempDirectory(), "config.json");
                IssueConfiguration config = new IssueConfiguration();
                Dictionary<FieldKey, FieldInfo> fields = new Dictionary<FieldKey, FieldInfo>(config.GetSchema("Issue"));
                config.Issues[0].Fields = new Dictionary<FieldKey, FieldInfo>();
                Assert.That(() => config.Save(file), Throws.TypeOf<AggregateException>());
                Assert.That(File.Exists(file), Is.False);

                config.Issues[0].Fields = new Dictionary<FieldKey, FieldInfo>(fields);
                config.Issues.Add(new IssueTypeConfiguration
                {
                    Name = "Issue",
                    Fields = new Dictionary<FieldKey, FieldInfo>(fields),
                });
                Assert.That(() => config.Save(file), Throws.TypeOf<AggregateException>());
                Assert.That(File.Exists(file), Is.False);
            }

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

                Assert.That(loaded.GetSchema("Issue").Keys, Is.EquivalentTo(config.GetSchema("Issue").Keys));
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(config.KeyProvider.Type));
                Assert.That(loaded.Storage, Is.EqualTo(IssueStorage.Markdown));
                Assert.That(json, Does.Contain("\"Storage\": \"Markdown\""));
                Assert.That(json, Does.Contain("\"KeyProvider\": \"DateIssueKeyProvider\""));
                Assert.That(json, Does.Contain("\"IssueType\": \"Issue\""));
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
                    KeyTemplate = "{type}-{uid}-{slug}",
                };
                config.Issues[0].Name = "FEAT";
                config.IssueType = "FEAT";

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

                Assert.That(json, Does.Contain("\"FieldType\": \"Value\""));
                Assert.That(json, Does.Contain("\"FieldType\": \"Array\""));
                Assert.That(json, Does.Not.Contain("Json"));
                Assert.That(json, Does.Not.Contain("\n  \"Fields\""));
                Assert.That(loaded.GetSchema("Issue")[FieldKey.Create("Title")].FieldType.ToString(), Is.EqualTo("Value"));
                Assert.That(loaded.GetSchema("Issue")[FieldKey.Create("Labels")].FieldType.ToString(), Is.EqualTo("Array"));
                Assert.That(loaded.GetSchema("Issue")[FieldKey.Create("Title")].FieldType.Type, Is.EqualTo(typeof(JsonValueField)));
                Assert.That(loaded.GetSchema("Issue")[FieldKey.Create("Title")].ValueType.Type, Is.EqualTo(typeof(GitIssue.Values.String)));
                Assert.That(loaded.GetSchema("Issue")[FieldKey.Create("Labels")].FieldType.Type, Is.EqualTo(typeof(JsonArrayField)));
                Assert.That(loaded.GetSchema("Issue")[FieldKey.Create("Labels")].ValueType.Type, Is.EqualTo(typeof(Label)));
            }

            [Test]
            public void Read_RejectsJsonFieldTypeNames()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                File.WriteAllText(file, """
                    {
                      "IssueType": "Issue",
                      "Storage": "Markdown",
                      "Issues": [
                        {
                          "Name": "Issue",
                          "Fields": {
                            "Title": {
                              "FieldType": "JsonValue",
                              "ValueType": "String",
                              "Required": true
                            }
                          }
                        }
                      ]
                    }
                    """);

                Assert.That(() => IssueConfiguration.Read(file), Throws.TypeOf<AggregateException>());
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
                      "IssueType": "Issue",
                      "Issues": [
                        {
                          "Name": "Issue",
                          "Fields": {
                            "Key": { "FieldType": "Value", "ValueType": "IssueKey", "Required": true },
                            "Title": { "FieldType": "Value", "ValueType": "String", "Required": true },
                            "Description": { "FieldType": "Value", "ValueType": "String", "Required": true },
                            "Author": { "FieldType": "Value", "ValueType": "Signature", "Required": true },
                            "Created": { "FieldType": "Value", "ValueType": "DateTime", "Required": true },
                            "Updated": { "FieldType": "Value", "ValueType": "DateTime", "Required": true }
                          }
                        }
                      ]
                    }
                    """);

                IssueConfiguration loaded = IssueConfiguration.Read(file);

#pragma warning disable CS0618
                Assert.That(loaded.KeyProvider.Type, Is.EqualTo(typeof(FileIssueKeyProvider)));
#pragma warning restore CS0618
            }

            [Test]
            public void Save_WritesStorageExplicitly()
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

                Assert.That(json, Does.Contain("\"Storage\": \"Json\""));
                Assert.That(loaded.Storage, Is.EqualTo(IssueStorage.Json));
            }

            [Test]
            public void Read_TreatsOmittedStorageAsMarkdown()
            {
                string directory = Helpers.CreateTempDirectory();
                string file = Path.Combine(directory, "config.json");
                File.WriteAllText(file, """
                    {
                      "IssueType": "Issue",
                      "Issues": [
                        {
                          "Name": "Issue",
                          "Fields": {
                            "Key": { "FieldType": "Value", "ValueType": "IssueKey", "Required": true },
                            "Title": { "FieldType": "Value", "ValueType": "String", "Required": true },
                            "Description": { "FieldType": "Value", "ValueType": "String", "Required": true },
                            "Author": { "FieldType": "Value", "ValueType": "Signature", "Required": true },
                            "Created": { "FieldType": "Value", "ValueType": "DateTime", "Required": true },
                            "Updated": { "FieldType": "Value", "ValueType": "DateTime", "Required": true }
                          }
                        }
                      ]
                    }
                    """);

                IssueConfiguration loaded = IssueConfiguration.Read(file);

                Assert.That(loaded.Storage, Is.EqualTo(IssueStorage.Markdown));
            }
        }
    }
}
