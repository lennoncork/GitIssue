using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.Formatting.SimpleFormatter
{
    [TestFixture]
    public partial class SimpleFormatterTests
    {
        [SetUp]
        public void Setup()
        {
            TestDirectory = Helpers.GetTempDirectory();
            if (Directory.Exists(TestDirectory) == false) Directory.CreateDirectory(TestDirectory);
        }

        protected string TestDirectory;

        protected string GitDirectory => Path.Combine(TestDirectory, Paths.GitFolderName);

        protected string IssueDirectory => Path.Combine(TestDirectory, Paths.IssueRootFolderName);

        protected string ConfigFile => Path.Combine(IssueDirectory, Paths.ConfigFileName);

        protected GitIssue.IssueManager Sut;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TestDirectory = Helpers.GetTestDirectory();
            if (Directory.Exists(TestDirectory)) Directory.Delete(TestDirectory, true);
            Directory.CreateDirectory(TestDirectory);
        }

        public void Initialize(
            string directory,
            bool initGit = true,
            bool initIssue = true,
            bool initSut = true)
        {
            if (initGit) Repository.Init(directory);
            if (initIssue) GitIssue.IssueManager.Init(directory);
            if (initSut) Sut = new GitIssue.IssueManager(TestDirectory);
        }

        public void Initialize(
            string directory,
            string name,
            bool initGit = true,
            bool initIssue = true,
            bool initSut = true)
        {
            if (initGit) Repository.Init(directory);
            if (initIssue) GitIssue.IssueManager.Init(directory, name);
            if (initSut) Sut = new GitIssue.IssueManager(TestDirectory, name);
        }
    }
}