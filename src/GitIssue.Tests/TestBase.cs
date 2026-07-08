using System;
using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tests
{
    public class TestsBase
    {
        protected virtual string ConfigFile => Path.Combine(this.IssueDirectory, Paths.ConfigFileName);

        protected virtual string GitDirectory => Path.Combine(this.TestDirectory, Paths.GitFolderName);

        protected virtual string IssueDirectory => Path.Combine(this.TestDirectory, Paths.IssueRootFolderName);

        protected virtual IIssueManager Manager { get; set; } = null!;

        protected virtual string TestDirectory { get; set; } = Helpers.GetTestDirectory();

        public void Initialize(
            string directory,
            bool initGit = true,
            bool initIssue = true,
            bool initSut = true)
        {
            if (initGit)
            {
                Repository.Init(directory);
            }

            if (initIssue)
            {
                IssueManager.Init(directory);
            }

            if (initSut)
            {
                this.Manager = IssueManager.Open(this.TestDirectory);
            }
        }

        public void Initialize(
            string directory,
            string name,
            bool initGit = true,
            bool initIssue = true,
            bool initSut = true)
        {
            if (initGit)
            {
                Repository.Init(directory);
            }

            if (initIssue)
            {
                IssueManager.Init(directory, name);
            }

            if (initSut)
            {
                this.Manager = IssueManager.Open(this.TestDirectory, name);
            }
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
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

        [SetUp]
        public virtual void Setup()
        {
            this.TestDirectory = Helpers.GetTempDirectory();
            if (!Directory.Exists(this.TestDirectory))
            {
                Directory.CreateDirectory(this.TestDirectory);
            }
        }
    }
}