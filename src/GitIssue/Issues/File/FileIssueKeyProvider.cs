using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace GitIssue.Issues.File
{
    /// <summary>
    ///     The issue key provider
    /// </summary>
    public class FileIssueKeyProvider : IssueKeyProvider
    {
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
        public override IssueKey Next()
        {
            var created = DateTime.Now;
            string[] values =
            {
                created.Year.ToString(),
                created.Month.ToString(),
                created.Day.ToString(),
                GetUniqueId(8)
            };
            var key = string.Join(Path.DirectorySeparatorChar.ToString(), values);
            return IssueKey.Create(key);
        }

        private string GetUniqueId(int length)
        {
            using (var sha = new SHA256Managed())
            {
                var checksum = sha.ComputeHash(Guid.NewGuid().ToByteArray());
                var key = BitConverter.ToString(checksum)
                    .Replace("-", "")
                    .ToUpperInvariant()
                    .Substring(0, length);
                return key;
            }
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
            var split = NormalizePath(value).Split('/', '\\');
            if (split.Length == 4)
            {
                var year = split[0];
                var month = split[1];
                var day = split[2];
                var id = split[3];

                if (DateTime.TryParse($"{year}/{month}/{day}", out var time))
                {
                    key = IssueKey.Create(value);
                    return true;
                }
            }

            key = IssueKey.None;
            return false;
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