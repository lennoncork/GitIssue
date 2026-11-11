using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class KeyProviderTests : IntegrationTestBase
    {
        [Test]
        public void Init_DefaultsToDateIssueKeyProvider()
        {
            Assert.That(this.Run("Init", "--storage", "json"), Is.Zero, this.Error);
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["KeyProvider"]?.GetValue<string>(), Is.EqualTo("DateIssueKeyProvider"));

            Assert.That(this.Run("Create", "Date key issue", "Stored with a date key."), Is.Zero, this.Error);
            string[] keys = this.GetIssueKeys();
            Assert.That(keys, Has.Length.EqualTo(1));
            Assert.That(keys[0], Does.Match(new Regex(@"^\d{4}-\d{2}-\d{2}-[A-F0-9]{8}$")));
        }

        [Test]
        public void Init_NameKeyProvider_CreatesNamedKeys()
        {
            Assert.That(this.Run("Init", "--key-provider", "name", "--storage", "json"), Is.Zero, this.Error);
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["KeyProvider"]?.GetValue<string>(), Is.EqualTo("NameIssueKeyProvider"));

            Assert.That(this.Run("Create", "Add login page", "Feature work."), Is.Zero, this.Error);

            string[] directories = Directory.EnumerateDirectories(this.IssuesDirectory)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Select(name => name!)
                .ToArray();
            Assert.That(directories, Has.Length.EqualTo(1));
            string key = directories[0];
            Assert.That(key, Does.Match(new Regex(@"^[A-F0-9]{8} - Add login page$")));
            this.AssertFileExists(Path.Combine(this.IssuesDirectory, key, $"{key}.json"));
            Assert.That(this.ReadJsonObject(Path.Combine(this.IssuesDirectory, key, $"{key}.json"))["Title"]?.GetValue<string>(), Is.EqualTo("Add login page"));
            Assert.That(this.Output, Does.Contain(key));
        }

        [Test]
        public void NameKeyProvider_DeleteByUniqueId()
        {
            Assert.That(this.Run("Init", "--key-provider", "name", "--storage", "json"), Is.Zero, this.Error);
            Assert.That(this.Run("Create", "Add login page", "Feature work."), Is.Zero, this.Error);

            string key = Directory.EnumerateDirectories(this.IssuesDirectory)
                .Select(Path.GetFileName)
                .Single(name => name != null)!;
            string uniqueId = key[..8];

            Assert.That(this.Run("Delete", uniqueId), Is.Zero, this.Error);
            Assert.That(Directory.Exists(Path.Combine(this.IssuesDirectory, key)), Is.False);
        }

        [Test]
        public void Init_Template_CreatesTemplatedKeysAndRekeyRenamesThem()
        {
            Assert.That(this.Run("Init", "--key-template", "{type}-{uid}-{slug}", "--issue-type", "FEAT", "--storage", "json"), Is.Zero, this.Error);
            Assert.That(this.Run("Create", "Add login page", "Feature work."), Is.Zero, this.Error);

            string[] directories = Directory.EnumerateDirectories(this.IssuesDirectory)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Select(name => name!)
                .ToArray();
            Assert.That(directories, Has.Length.EqualTo(1));
            string original = directories[0];
            Assert.That(original, Does.Match(new Regex(@"^FEAT-[A-F0-9]{8}-add-login-page$")));

            string json = File.ReadAllText(this.ConfigurationFile);
            File.WriteAllText(this.ConfigurationFile, json.Replace("{type}-{uid}-{slug}", "{uid} - {name}", System.StringComparison.Ordinal));

            Assert.That(this.Run("Rekey", "--dry-run"), Is.Zero, this.Error);
            Assert.That(this.Output, Does.Contain(original));
            Assert.That(Directory.Exists(Path.Combine(this.IssuesDirectory, original)), Is.True);

            Assert.That(this.Run("Rekey"), Is.Zero, this.Error);
            string[] renamed = Directory.EnumerateDirectories(this.IssuesDirectory)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Select(name => name!)
                .ToArray();
            Assert.That(renamed, Has.Length.EqualTo(1));
            Assert.That(renamed[0], Does.Match(new Regex(@"^[A-F0-9]{8} - Add login page$")));
            Assert.That(Directory.Exists(Path.Combine(this.IssuesDirectory, original)), Is.False);
        }
    }
}
