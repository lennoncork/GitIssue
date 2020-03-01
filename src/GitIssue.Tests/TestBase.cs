using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests
{
    
    public class TestsBase
    {
        [SetUp]
        public virtual void Setup()
        {
            TestDirectory = Helpers.GetTempDirectory();
            if (Directory.Exists(TestDirectory) == false) Directory.CreateDirectory(TestDirectory);
        }

        protected virtual string TestDirectory { get; set; } = Helpers.GetTestDirectory();

        protected virtual string GitDirectory => Path.Combine(TestDirectory, Paths.GitFolderName);

        protected virtual string IssueDirectory => Path.Combine(TestDirectory, Paths.IssueRootFolderName);

        protected virtual string ConfigFile => Path.Combine(IssueDirectory, Paths.ConfigFileName);

        protected virtual IssueManager Manager { get; set; }

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            TestDirectory = Helpers.GetTestDirectory();
            if (Directory.Exists(TestDirectory)) 
                Directory.Delete(TestDirectory, true);
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
            if (initSut) Manager = new GitIssue.IssueManager(TestDirectory);
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
            if (initSut) Manager = new GitIssue.IssueManager(TestDirectory, name);
        }
    }
}