using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    [TestFixture]
    public partial class BugIntegrationTests
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

        protected IRepository GitRepository { get; set; }

        protected IIssueManager Issues { get; set; }

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
            bool initIssue = true)
        {
            if (initGit)
                GitRepository = new Repository(Repository.Init(directory));

            if (initIssue)
                Issues = GitIssue.IssueManager.Init(new BugConfiguration(), directory);
        }
    }
}