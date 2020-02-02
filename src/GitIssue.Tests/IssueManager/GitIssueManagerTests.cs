using System.IO;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        protected string TestDirectory;

        protected string GitDirectory => Path.Combine(this.TestDirectory, Paths.GitFolderName);

        protected string IssueDirectory => Path.Combine(this.TestDirectory, Paths.IssueRootFolderName);

        protected string ConfigFile => Path.Combine(this.IssueDirectory, Paths.ConfigFileName);

        protected GitIssue.IssueManager Sut;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.TestDirectory = Helpers.GetTestDirectory();
            if (Directory.Exists(this.TestDirectory))
            {
                Directory.Delete(this.TestDirectory, true);
            }
            Directory.CreateDirectory(this.TestDirectory);
        }

        [SetUp]
        public void Setup()
        {
            this.TestDirectory = Helpers.GetTempDirectory();
            if (Directory.Exists(this.TestDirectory) == false)
            {
                Directory.CreateDirectory(this.TestDirectory);
            }
        }

        public void Initialize(
            string directory,
            bool initGit = true, 
            bool initIssue = true,
            bool initSut = true)
        {
            if(initGit) LibGit2Sharp.Repository.Init(directory);
            if(initIssue) GitIssue.IssueManager.Init(directory);
            if(initSut) this.Sut = new GitIssue.IssueManager(this.TestDirectory);
        }

        public void Initialize(
            string directory,
            string name,
            bool initGit = true,
            bool initIssue = true,
            bool initSut = true)
        {
            if (initGit) LibGit2Sharp.Repository.Init(directory);
            if (initIssue) GitIssue.IssueManager.Init(directory, name);
            if (initSut) this.Sut = new GitIssue.IssueManager(this.TestDirectory, name);
        }
    }
}
