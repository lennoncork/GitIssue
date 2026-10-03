using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [NonParallelizable]
    public abstract class IntegrationTestBase
    {
        protected string Error { get; private set; } = string.Empty;

        protected string ChangesFile => Path.Combine(this.IssuesDirectory, Paths.ChangeLogFileName);

        protected string ConfigurationFile => Path.Combine(this.IssuesDirectory, Paths.ConfigFileName);

        protected string IssuesDirectory => Path.Combine(this.TestDirectory, Paths.IssueRootFolderName);

        protected string Output { get; private set; } = string.Empty;

        protected string TestDirectory { get; private set; } = null!;

        protected string TrackingFile => Path.Combine(this.IssuesDirectory, Paths.TrackedIssueFileName);

        [SetUp]
        public void Setup()
        {
            this.TestDirectory = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                nameof(GitIssue.Tool.Tests),
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.TestDirectory);

            using Repository repository = new Repository(Repository.Init(this.TestDirectory));
            repository.Config.Set("user.name", "GitIssue Tool Tests");
            repository.Config.Set("user.email", "gitissue-tool-tests@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Directory.Delete(this.TestDirectory, true);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        protected string CreateIssue(string title, string description = "")
        {
            Assert.That(this.Run("Create", title, description), Is.Zero, this.Error);

            return this.FindIssueKey(title);
        }

        protected string FindIssueKey(string title)
        {
            string[] keys = this.GetIssueKeys()
                .Where(key => this.ReadIssue(key)["Title"]?.GetValue<string>() == title)
                .ToArray();
            Assert.That(keys, Has.Length.EqualTo(1), $"Expected one issue with title '{title}'.");
            return keys[0];
        }

        protected void InitializeRepository()
        {
            Assert.That(this.Run("Init"), Is.Zero, this.Error);
        }

        protected void AssertDirectoryDoesNotExist(string path)
        {
            Assert.That(Directory.Exists(path), Is.False, $"Directory should not exist: {path}");
        }

        protected void AssertDirectoryExists(string path)
        {
            Assert.That(Directory.Exists(path), Is.True, $"Directory should exist: {path}");
        }

        protected void AssertFileDoesNotExist(string path)
        {
            Assert.That(File.Exists(path), Is.False, $"File should not exist: {path}");
        }

        protected void AssertFileExists(string path)
        {
            Assert.That(File.Exists(path), Is.True, $"File should exist: {path}");
        }

        protected void AssertFileContents(string path, string expected)
        {
            Assert.That(this.ReadFile(path), Is.EqualTo(expected));
        }

        protected void AssertIssueDoesNotExist(string key)
        {
            this.AssertDirectoryDoesNotExist(this.GetIssueDirectory(key));
            this.AssertFileDoesNotExist(this.GetIssueFile(key));
        }

        protected void AssertIssueExists(string key)
        {
            this.AssertDirectoryExists(this.GetIssueDirectory(key));
            this.AssertFileExists(this.GetIssueFile(key));
        }

        protected void AssertJsonArrayContains(JsonObject json, string property, string expected)
        {
            JsonArray? values = json[property] as JsonArray;
            Assert.That(values, Is.Not.Null, $"Expected '{property}' to be an array.");
            if (values == null)
            {
                return;
            }

            Assert.That(values.Select(value => value?.GetValue<string>()), Does.Contain(expected));
        }

        protected void AssertJsonArrayDoesNotContain(JsonObject json, string property, string expected)
        {
            JsonArray? values = json[property] as JsonArray;
            Assert.That(values, Is.Not.Null, $"Expected '{property}' to be an array.");
            if (values == null)
            {
                return;
            }

            Assert.That(values.Select(value => value?.GetValue<string>()), Does.Not.Contain(expected));
        }

        protected void AssertJsonArrayItemContains(JsonObject json, string property, string expected)
        {
            JsonArray? values = json[property] as JsonArray;
            Assert.That(values, Is.Not.Null, $"Expected '{property}' to be an array.");
            if (values == null)
            {
                return;
            }

            Assert.That(values.Select(value => value?.GetValue<string>()), Has.Some.Contains(expected));
        }

        protected void AssertJsonPropertyExists(JsonObject json, string property)
        {
            Assert.That(json[property], Is.Not.Null, $"Expected JSON property '{property}'.");
        }

        protected void AssertJsonValue(JsonObject json, string property, string expected)
        {
            Assert.That(json[property]?.GetValue<string>(), Is.EqualTo(expected));
        }

        protected string GetIssueDirectory(string key)
        {
            return Path.Combine(this.IssuesDirectory, key.Replace('-', Path.DirectorySeparatorChar));
        }

        protected string GetIssueFile(string key)
        {
            return Path.Combine(this.GetIssueDirectory(key), $"{key}.json");
        }

        protected string[] GetIssueKeys()
        {
            if (!Directory.Exists(this.IssuesDirectory))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(this.IssuesDirectory, "*.json", SearchOption.AllDirectories)
                .Select(file => new
                {
                    File = file,
                    Parts = Path.GetRelativePath(this.IssuesDirectory, Path.GetDirectoryName(file)!)
                        .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries),
                })
                .Select(entry => new { entry.File, entry.Parts, Key = string.Join("-", entry.Parts) })
                .Where(entry => entry.Parts.Length == 4 && Path.GetFileNameWithoutExtension(entry.File) == entry.Key)
                .Select(entry => entry.Key)
                .ToArray();
        }

        protected JsonObject ReadIssue(string key)
        {
            return this.ReadJsonObject(this.GetIssueFile(key));
        }

        protected string ReadFile(string path)
        {
            this.AssertFileExists(path);
            return File.ReadAllText(path);
        }

        protected JsonObject ReadJsonObject(string path)
        {
            JsonNode? json = JsonNode.Parse(this.ReadFile(path));
            Assert.That(json, Is.InstanceOf<JsonObject>(), $"Expected JSON object in {path}");
            return json!.AsObject();
        }

        protected void RunSuccessfully(params string[] arguments)
        {
            Assert.That(this.Run(arguments), Is.Zero, this.Error);
        }

        protected int Run(params string[] arguments)
        {
            TextWriter originalOut = Console.Out;
            TextWriter originalError = Console.Error;
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            try
            {
                Console.SetOut(output);
                Console.SetError(error);
                string[] commandArguments = arguments
                    .Concat(new[] { "--path", this.TestDirectory })
                    .ToArray();
                return Program.Main(commandArguments);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
                this.Output = Regex.Replace(output.ToString(), "\u001B\\[[0-9;]*m", string.Empty);
                this.Error = error.ToString();
            }
        }
    }
}