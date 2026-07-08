using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LibGit2Sharp;

namespace GitIssue.Issues.File
{
    /// <summary>
    ///     The issue key provider
    /// </summary>
    public class FileIssueKeyProvider : IssueKeyProvider
    {
        private static readonly char separator = '-';

        private readonly RepositoryRoot root;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root"></param>
        public FileIssueKeyProvider(RepositoryRoot root)
        {
            this.root = root;
        }

        /// <inheritdoc />
        public override IEnumerable<IssueKey> Keys => this.FindAll(this.root.IssuesPath);

        /// <inheritdoc />
        public override string GetIssuePath(IssueKey key)
        {
            return key.ToString().Replace(FileIssueKeyProvider.separator, Path.DirectorySeparatorChar);
        }

        /// <inheritdoc />
        public override IssueKey Next()
        {
            DateTime created = DateTime.Now;
            string[] values = { created.Year.ToString("D4"), created.Month.ToString("D2"), created.Day.ToString("D2"), this.GetUniqueId(8) };
            string key = string.Join(FileIssueKeyProvider.separator, values);
            return IssueKey.Create(key);
        }

        /// <summary>
        ///     Tries to get the commit of the current branch
        /// </summary>
        /// <param name="commit"></param>
        /// <returns></returns>
        public bool TryGetGitCommit(out string commit)
        {
            using IRepository repository = this.root.GetRepository();
            commit = repository.Head.Tip.Sha;
            return true;
        }

        /// <inheritdoc />
        public override bool TryGetKey(string value, out IssueKey key)
        {
            value = value.Replace(FileIssueKeyProvider.separator, Path.DirectorySeparatorChar);
            string[] split = this.NormalizePath(value).Split('/', '\\');
            if (split.Length == 4)
            {
                string year = split[0];
                string month = split[1];
                string day = split[2];
                string id = split[3];

                if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime time))
                {
                    key = IssueKey.Create(value.Replace(Path.DirectorySeparatorChar, FileIssueKeyProvider.separator));
                    return true;
                }
            }

            key = IssueKey.None;
            return false;
        }

        private IEnumerable<IssueKey> FindAll(string directory)
        {
            List<IssueKey> keys = new List<IssueKey>();

            if (!Directory.Exists(this.root.IssuesPath))
            {
                return keys;
            }

            foreach (DirectoryInfo year in Directory.EnumerateDirectories(this.root.IssuesPath)
                         .Select(d => new DirectoryInfo(d)))
            foreach (DirectoryInfo month in Directory.EnumerateDirectories(year.FullName)
                         .Select(d => new DirectoryInfo(d)))
            foreach (DirectoryInfo day in Directory.EnumerateDirectories(month.FullName)
                         .Select(d => new DirectoryInfo(d)))
            foreach (DirectoryInfo id in Directory.EnumerateDirectories(day.FullName)
                         .Select(d => new DirectoryInfo(d)))
            {
                string path = Path.Combine(year.Name, month.Name, day.Name, id.Name);
                if (this.TryGetKey(path, out IssueKey key))
                {
                    keys.Add(key);
                }
            }

            return keys;
        }

        private string GetUniqueId(int length)
        {
            using SHA256 sha = SHA256.Create();
            byte[] checksum = sha.ComputeHash(Guid.NewGuid().ToByteArray());
            string key = BitConverter.ToString(checksum)
                .Replace("-", "")
                .ToUpperInvariant()
                .Substring(0, length);
            return key;
        }

        private string NormalizePath(string path)
        {
            if (Directory.Exists(Path.Combine(this.root.IssuesPath, path)))
            {
                return path;
            }

            if (this.root.IssuesPath.Contains(Path.GetFullPath(path)))
            {
                return this.root.IssuesPath
                    .Remove(0, this.root.IssuesPath.Length)
                    .Trim('/', '\\');
            }

            return string.Empty;
        }
    }
}