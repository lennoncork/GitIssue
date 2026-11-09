using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitIssue.Values;
using LibGit2Sharp;
using DateTime = System.DateTime;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Generates date-based issue keys in the form YYYY-MM-DD-UID
    /// </summary>
    [TypeAlias(nameof(DateIssueKeyProvider))]
    public class DateIssueKeyProvider : IssueKeyProvider
    {
        private static readonly char separator = '-';

        private readonly RepositoryRoot root;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        public DateIssueKeyProvider(RepositoryRoot root)
        {
            this.root = root;
        }

        /// <inheritdoc />
        public override IEnumerable<IssueKey> Keys => this.FindAll(this.root.IssuesPath);

        /// <inheritdoc />
        public override string GetIssuePath(IssueKey key)
        {
            return key.ToString().Replace(DateIssueKeyProvider.separator, Path.DirectorySeparatorChar);
        }

        /// <inheritdoc />
        public override IssueKey Next()
        {
            DateTime created = DateTime.Now;
            string[] values =
            {
                created.Year.ToString("D4"),
                created.Month.ToString("D2"),
                created.Day.ToString("D2"),
                IssueKeyProvider.GetUniqueId(IssueKeyProvider.UniqueIdLength),
            };
            string key = string.Join(DateIssueKeyProvider.separator, values);
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
            value = value.Replace(DateIssueKeyProvider.separator, Path.DirectorySeparatorChar);
            string[] split = this.NormalizePath(value).Split('/', '\\');
            if (split.Length == 4)
            {
                string year = split[0];
                string month = split[1];
                string day = split[2];

                if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime _))
                {
                    key = IssueKey.Create(value.Replace(Path.DirectorySeparatorChar, DateIssueKeyProvider.separator));
                    return true;
                }
            }

            key = IssueKey.None;
            return false;
        }

        private IEnumerable<IssueKey> FindAll(string directory)
        {
            List<IssueKey> keys = new List<IssueKey>();

            if (!Directory.Exists(directory))
            {
                return keys;
            }

            foreach (DirectoryInfo year in Directory.EnumerateDirectories(directory)
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
