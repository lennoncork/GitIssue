using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Keys;

namespace GitIssue.Issues
{
    public class FileIssueKeyProvider : IssueKeyProvider
    {
        private readonly RepositoryRoot root;

        public FileIssueKeyProvider(RepositoryRoot root)
        {
            this.root = root;
        }

        /// <inheritdoc/>
        public override IEnumerable<IssueKey> Keys => this.FindAll(this.root.IssuesPath);

        /// <inheritdoc/>
        public override IssueKey Next()
        {
            DateTime created = DateTime.Now;
            string[] values = {
                created.Year.ToString(),
                created.Month.ToString(),
                created.Day.ToString(),
                this.GetUniqueId(8)
            };
            string key = string.Join(Path.DirectorySeparatorChar.ToString(), values);
            return IssueKey.Create(key);
        }

        private string GetUniqueId(int length)
        {
            using (var sha = new SHA256Managed())
            {
                byte[] checksum = sha.ComputeHash(Guid.NewGuid().ToByteArray());
                string key = BitConverter.ToString(checksum)
                    .Replace("-", "")
                    .ToUpperInvariant()
                    .Substring(0, length);
                return key;
            }
        }

        private IEnumerable<IssueKey> FindAll(string directory)
        {
            List<IssueKey> keys = new List<IssueKey>();
            
            if (Directory.Exists(root.IssuesPath) == false)
                return keys;

            foreach (DirectoryInfo year in Directory.EnumerateDirectories(root.IssuesPath)
                .Select(d => new DirectoryInfo(d)))
            {
                foreach (DirectoryInfo month in Directory.EnumerateDirectories(year.FullName)
                    .Select(d => new DirectoryInfo(d)))
                {
                    foreach (DirectoryInfo day in Directory.EnumerateDirectories(month.FullName)
                        .Select(d => new DirectoryInfo(d)))
                    {
                        foreach (DirectoryInfo id in Directory.EnumerateDirectories(day.FullName)
                            .Select(d => new DirectoryInfo(d)))
                        {
                            string path = Path.Combine(year.Name, month.Name, day.Name, id.Name);
                            if (this.TryGetKey(path, out IssueKey key))
                                keys.Add(key);
                        }
                    }
                }
            }
            
            return keys;
        }

        /// <inheritdoc/>
        public override bool TryGetKey(string value, out IssueKey key)
        {
            string[] split = this.NormalizePath(value).Split(new []{'/', '\\'});
            if (split.Length == 4)
            {
                string year = split[0];
                string month = split[1];
                string day = split[2];
                string id = split[3];

                if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime time))
                {
                    key = IssueKey.Create(value);
                    return true;
                }
            }
            key = IssueKey.None();
            return false;
        }

        private string NormalizePath(string path)
        {
            if (Directory.Exists(Path.Combine(this.root.IssuesPath, path)))
                return path;

            if (this.root.IssuesPath.Contains(Path.GetFullPath(path)))
            {
                return this.root.IssuesPath
                    .Remove(0, this.root.IssuesPath.Length)
                    .Trim(new[] {'/', '\\'});
            }

            return string.Empty;
        }
    }
}
