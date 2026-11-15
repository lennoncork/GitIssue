using System;
using System.IO;
using GitIssue.Services;
using LibGit2Sharp;

namespace GitIssue.Services.Web.Tests
{
    internal sealed class RepositoryFixture : IDisposable
    {
        public RepositoryFixture()
        {
            this.Directory = Path.Combine(Path.GetTempPath(), "gitissue-web-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(this.Directory);
            Repository.Init(this.Directory);
            using Repository repository = new Repository(this.Directory);
            repository.Config.Set("user.name", "Tester");
            repository.Config.Set("user.email", "tester@example.com");
            using IIssueManager manager = IssueManager.Init(this.Directory);
        }

        public string Directory { get; }

        public IssueWorkspace Open()
        {
            return new IssueWorkspace(IssueManager.Open(this.Directory));
        }

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(this.Directory, true);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
