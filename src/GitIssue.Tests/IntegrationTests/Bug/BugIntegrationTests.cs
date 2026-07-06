using System;
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
            this.TestDirectory = Helpers.GetTempDirectory();
            if (!Directory.Exists(this.TestDirectory))
            {
                Directory.CreateDirectory(this.TestDirectory);
            }
        }

        protected string TestDirectory = Path.GetTempPath();

        protected string GitDirectory => Path.Combine(this.TestDirectory, Paths.GitFolderName);

        protected string IssueDirectory => Path.Combine(this.TestDirectory, Paths.IssueRootFolderName);

        protected string ConfigFile => Path.Combine(this.IssueDirectory, Paths.ConfigFileName);

        protected IRepository GitRepository { get; set; } = null!;

        protected IIssueManager Issues { get; set; } = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.TestDirectory = Helpers.GetTestDirectory();
            if (Directory.Exists(this.TestDirectory))
            {
                try
                {
                    Directory.Delete(this.TestDirectory, true);
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore if cleanup cannot delete some files; per-test temp
                    // directories created in Setup() ensure isolation.
                }
            }

            Directory.CreateDirectory(this.TestDirectory);
        }

        public void Initialize(
            string directory,
            bool initGit = true,
            bool initIssue = true)
        {
            if (initGit)
            {
                this.GitRepository = new Repository(Repository.Init(directory));
            }

            if (initIssue)
            {
                this.Issues = IssueManager.Init(new BugConfiguration(), directory);
            }
        }
    }
}