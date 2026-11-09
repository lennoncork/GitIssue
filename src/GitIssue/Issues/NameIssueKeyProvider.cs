using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitIssue.Values;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Generates named issue keys in the form "UID - Issue Name"
    /// </summary>
    [TypeAlias(nameof(NameIssueKeyProvider))]
    public class NameIssueKeyProvider : IssueKeyProvider
    {
        private const string separator = " - ";

        private const int maximumNameLength = 80;

        private readonly RepositoryRoot root;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NameIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        public NameIssueKeyProvider(RepositoryRoot root)
        {
            this.root = root;
        }

        /// <inheritdoc />
        public override IEnumerable<IssueKey> Keys => this.FindAll();

        /// <inheritdoc />
        public override string GetIssuePath(IssueKey key)
        {
            return key.ToString();
        }

        /// <inheritdoc />
        public override IssueKey Next()
        {
            return this.Next("Untitled");
        }

        /// <inheritdoc />
        public override IssueKey Next(string name)
        {
            string uid = IssueKeyProvider.GetUniqueId(IssueKeyProvider.UniqueIdLength);
            string sanitized = NameIssueKeyProvider.SanitizeName(name);
            return IssueKey.Create($"{uid}{NameIssueKeyProvider.separator}{sanitized}");
        }

        /// <inheritdoc />
        public override bool TryGetKey(string value, out IssueKey key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                key = IssueKey.None;
                return false;
            }

            string normalized = value.Trim();
            if (NameIssueKeyProvider.TryParseKey(normalized, out key))
            {
                return true;
            }

            string fileName = Path.GetFileName(normalized.TrimEnd('/', '\\'));
            if (!string.IsNullOrEmpty(fileName) &&
                (fileName != normalized) &&
                NameIssueKeyProvider.TryParseKey(fileName, out key))
            {
                return true;
            }

            if (NameIssueKeyProvider.IsUniqueId(normalized))
            {
                foreach (IssueKey existing in this.Keys)
                {
                    if (existing.ToString().StartsWith($"{normalized}{NameIssueKeyProvider.separator}", StringComparison.OrdinalIgnoreCase))
                    {
                        key = existing;
                        return true;
                    }
                }
            }

            key = IssueKey.None;
            return false;
        }

        private static bool IsUniqueId(string value)
        {
            return (value.Length == IssueKeyProvider.UniqueIdLength) && value.All(Uri.IsHexDigit);
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Untitled";
            }

            HashSet<char> invalid = Path.GetInvalidFileNameChars()
                .Concat(new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' })
                .ToHashSet();
            StringBuilder builder = new StringBuilder(name.Length);
            bool pendingSpace = false;

            foreach (char character in name.Trim())
            {
                if (invalid.Contains(character) || char.IsControl(character))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (char.IsWhiteSpace(character))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(character);
            }

            string sanitized = builder.ToString().TrimEnd('.', ' ');
            if (string.IsNullOrEmpty(sanitized) || (sanitized == ".") || (sanitized == ".."))
            {
                return "Untitled";
            }

            if (sanitized.Length > NameIssueKeyProvider.maximumNameLength)
            {
                sanitized = sanitized.Substring(0, NameIssueKeyProvider.maximumNameLength).TrimEnd('.', ' ');
            }

            return string.IsNullOrEmpty(sanitized) ? "Untitled" : sanitized;
        }

        private static bool TryParseKey(string value, out IssueKey key)
        {
            int separatorIndex = value.IndexOf(NameIssueKeyProvider.separator, StringComparison.Ordinal);
            if (separatorIndex == IssueKeyProvider.UniqueIdLength)
            {
                string uid = value.Substring(0, separatorIndex);
                string name = value.Substring(separatorIndex + NameIssueKeyProvider.separator.Length);
                if (NameIssueKeyProvider.IsUniqueId(uid) && !string.IsNullOrWhiteSpace(name))
                {
                    key = IssueKey.Create(value);
                    return true;
                }
            }

            key = IssueKey.None;
            return false;
        }

        private IEnumerable<IssueKey> FindAll()
        {
            List<IssueKey> keys = new List<IssueKey>();

            if (!Directory.Exists(this.root.IssuesPath))
            {
                return keys;
            }

            foreach (string directory in Directory.EnumerateDirectories(this.root.IssuesPath))
            {
                string name = Path.GetFileName(directory);
                if (NameIssueKeyProvider.TryParseKey(name, out IssueKey key))
                {
                    keys.Add(key);
                }
            }

            return keys;
        }
    }
}
