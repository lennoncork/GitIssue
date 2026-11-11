using System;
using System.IO;
using NUnit.Framework;

namespace GitIssue.Services.Mcp.Tests
{
    public sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            this.Path = System.IO.Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                nameof(GitIssue.Services.Mcp.Tests),
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(this.Path, true);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
