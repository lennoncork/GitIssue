using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    [TestFixture]
    public partial class BugIntegrationTests
    {
        protected string TestDirectory;

        protected string GitDirectory => Path.Combine(this.TestDirectory, Paths.GitFolderName);

        protected string IssueDirectory => Path.Combine(this.TestDirectory, Paths.IssueRootFolderName);

        protected string ConfigFile => Path.Combine(this.IssueDirectory, Paths.ConfigFileName);

        protected IRepository GitRepository { get; set; }

        protected IIssueManager Issues {get; set; }

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
            bool initIssue = true)
        {
            if (initGit) 
                this.GitRepository = new Repository(Repository.Init(directory));

            if (initIssue) 
                this.Issues = GitIssue.IssueManager.Init(new BugConfiguration(), directory);
        }
    }
}