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
        private static char separator = '-';

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
        public override IEnumerable<IssueKey> Keys => FindAll(root.IssuesPath);

        /// <inheritdoc />
        public override string GetIssuePath(IssueKey key)
        {
            return key.ToString().Replace(separator, Path.DirectorySeparatorChar);
        }

        /// <inheritdoc />
        public override IssueKey Next()
        {
            var created = DateTime.Now;
            string[] values =
            {
                created.Year.ToString("D4"),
                created.Month.ToString("D2"),
                created.Day.ToString("D2"),
                GetUniqueId(8)
            };
            var key = string.Join(separator, values);
            return IssueKey.Create(key);
        }

        private string GetUniqueId(int length)
        {
            using var sha = new SHA256Managed();
            var checksum = sha.ComputeHash(Guid.NewGuid().ToByteArray());
            var key = BitConverter.ToString(checksum)
                .Replace("-", "")
                .ToUpperInvariant()
                .Substring(0, length);
            return key;
        }

        private IEnumerable<IssueKey> FindAll(string directory)
        {
            var keys = new List<IssueKey>();

            if (Directory.Exists(root.IssuesPath) == false)
                return keys;

            foreach (var year in Directory.EnumerateDirectories(root.IssuesPath)
                .Select(d => new DirectoryInfo(d)))
            foreach (var month in Directory.EnumerateDirectories(year.FullName)
                .Select(d => new DirectoryInfo(d)))
            foreach (var day in Directory.EnumerateDirectories(month.FullName)
                .Select(d => new DirectoryInfo(d)))
            foreach (var id in Directory.EnumerateDirectories(day.FullName)
                .Select(d => new DirectoryInfo(d)))
            {
                var path = Path.Combine(year.Name, month.Name, day.Name, id.Name);
                if (TryGetKey(path, out var key))
                    keys.Add(key);
            }

            return keys;
        }

        /// <inheritdoc />
        public override bool TryGetKey(string value, out IssueKey key)
        {
            value = value.Replace(separator, Path.DirectorySeparatorChar);
            var split = NormalizePath(value).Split('/', '\\');
            if (split.Length == 4)
            {
                var year = split[0];
                var month = split[1];
                var day = split[2];
                var id = split[3];

                if (DateTime.TryParse($"{year}/{month}/{day}", out var time))
                {
                    key = IssueKey.Create(value.Replace(Path.DirectorySeparatorChar, separator));
                    return true;
                }
            }

            key = IssueKey.None;
            return false;
        }

        /// <summary>
        /// Tries to get the commit of the current branch
        /// </summary>
        /// <param name="commit"></param>
        /// <returns></returns>
        public bool TryGetGitCommit(out string commit)
        {
            using IRepository repository = root.GetRepository();
            commit = repository.Head.Tip.Sha;
            return true;
        }

        private string NormalizePath(string path)
        {
            if (Directory.Exists(Path.Combine(root.IssuesPath, path)))
                return path;

            if (root.IssuesPath.Contains(Path.GetFullPath(path)))
                return root.IssuesPath
                    .Remove(0, root.IssuesPath.Length)
                    .Trim('/', '\\');

            return string.Empty;
        }
    }
}