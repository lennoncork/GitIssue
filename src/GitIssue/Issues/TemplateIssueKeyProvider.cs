using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitIssue.Values;
using DateTime = System.DateTime;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Generates issue keys from a configurable template
    /// </summary>
    [TypeAlias(nameof(TemplateIssueKeyProvider))]
    public class TemplateIssueKeyProvider : IssueKeyProvider
    {
        private static readonly Regex UniqueIdPattern = new Regex(
            $"[A-Fa-f0-9]{{{IssueKeyProvider.UniqueIdLength}}}",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly IIssueConfiguration configuration;

        private readonly string fallbackTemplate;

        /// <summary>
        ///     Gets the repository root
        /// </summary>
        protected RepositoryRoot Root { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemplateIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="configuration">the issue configuration</param>
        public TemplateIssueKeyProvider(RepositoryRoot root, IIssueConfiguration configuration)
            : this(root, configuration, IssueKeyTemplate.Date)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemplateIssueKeyProvider" /> class
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="configuration">the issue configuration</param>
        /// <param name="fallbackTemplate">the template used when configuration does not specify one</param>
        protected TemplateIssueKeyProvider(RepositoryRoot root, IIssueConfiguration configuration, string fallbackTemplate)
        {
            this.Root = root;
            this.configuration = configuration;
            this.fallbackTemplate = fallbackTemplate;
        }

        /// <summary>
        ///     Extracts an 8-character unique id from a key string
        /// </summary>
        /// <param name="value">the key</param>
        /// <returns>the unique id, or null when none is present</returns>
        public static string? ExtractUid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            MatchCollection matches = TemplateIssueKeyProvider.UniqueIdPattern.Matches(value);
            return matches.Count == 0 ? null : matches[matches.Count - 1].Value.ToUpperInvariant();
        }

        /// <summary>
        ///     Gets the parsed key template
        /// </summary>
        public IssueKeyTemplate Template => IssueKeyTemplate.Parse(this.TemplateSource);

        /// <summary>
        ///     Gets the template string in effect
        /// </summary>
        public string TemplateSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.configuration.KeyTemplate) ||
                    ((this.configuration.KeyTemplate == IssueKeyTemplate.Date) &&
                     (this.fallbackTemplate != IssueKeyTemplate.Date)))
                {
                    return this.fallbackTemplate;
                }

                return this.configuration.KeyTemplate;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IssueKey> Keys => this.FindAll();

        /// <inheritdoc />
        public override string GetIssuePath(IssueKey key)
        {
            if (this.Template.TryMatch(key.ToString(), out IssueKeyContext context))
            {
                return this.Template.RenderPath(context);
            }

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
            return this.Create(this.CreateContext(name));
        }

        /// <summary>
        ///     Creates a key from substitution values
        /// </summary>
        /// <param name="context">the substitution values</param>
        /// <returns>the issue key</returns>
        public IssueKey Create(IssueKeyContext context)
        {
            IssueKeyContext complete = new IssueKeyContext
            {
                Type = IssueTypeName.Validate(context.Type),
                Created = context.Created == default ? DateTime.Now : context.Created,
                Index = context.Index > 0 ? context.Index : this.NextIndex(),
                Uid = string.IsNullOrEmpty(context.Uid) ? IssueKeyProvider.GetUniqueId(IssueKeyProvider.UniqueIdLength) : context.Uid,
                Name = string.IsNullOrWhiteSpace(context.Name) ? "Untitled" : context.Name,
            };

            return IssueKey.Create(this.Template.RenderKey(complete));
        }

        /// <summary>
        ///     Builds substitution values for a new issue
        /// </summary>
        /// <param name="name">the issue name or title</param>
        /// <returns>the substitution values</returns>
        public IssueKeyContext CreateContext(string name)
        {
            return new IssueKeyContext
            {
                Type = this.configuration.IssueType,
                Created = DateTime.Now,
                Index = this.NextIndex(),
                Uid = IssueKeyProvider.GetUniqueId(IssueKeyProvider.UniqueIdLength),
                Name = name,
            };
        }

        /// <summary>
        ///     Builds substitution values for an existing issue
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <returns>the substitution values</returns>
        public IssueKeyContext CreateContext(IIssue issue)
        {
            IssueKeyTemplate template = this.Template;
            bool matchedCurrentKey = template.TryMatch(issue.Key.ToString(), out IssueKeyContext matched);
            string title = issue.Title.ToString();
            DateTime created = (DateTime)issue.Created;
            return new IssueKeyContext
            {
                Type = string.IsNullOrEmpty(issue.IssueType)
                    ? (matchedCurrentKey && template.HasType ? matched.Type : this.configuration.IssueType)
                    : issue.IssueType,
                Created = created == default ? (matched.Created == default ? DateTime.Now : matched.Created) : created,
                Index = matched.Index,
                Uid = string.IsNullOrEmpty(matched.Uid)
                    ? TemplateIssueKeyProvider.ExtractUid(issue.Key.ToString()) ?? IssueKeyProvider.GetUniqueId(IssueKeyProvider.UniqueIdLength)
                    : matched.Uid,
                Name = string.IsNullOrWhiteSpace(title) ? (string.IsNullOrWhiteSpace(matched.Name) ? "Untitled" : matched.Name) : title,
            };
        }

        /// <summary>
        ///     The next sequential index for templates that include {index}
        /// </summary>
        /// <returns>the next index</returns>
        public int NextIndex()
        {
            int maximum = 0;
            foreach (IssueKey key in this.Keys)
            {
                if (this.Template.TryMatch(key.ToString(), out IssueKeyContext context) && (context.Index > maximum))
                {
                    maximum = context.Index;
                }
            }

            return maximum + 1;
        }

        /// <inheritdoc />
        public override bool TryGetKey(string value, out IssueKey key)
        {
            if (this.TryGetContext(value, out IssueKeyContext context))
            {
                key = IssueKey.Create(this.Template.RenderKey(context));
                return true;
            }

            if (!string.IsNullOrWhiteSpace(value) &&
                (value.Trim().Length == IssueKeyProvider.UniqueIdLength) &&
                value.Trim().All(Uri.IsHexDigit))
            {
                string uid = value.Trim();
                foreach (IssueKey existing in this.Keys)
                {
                    if (this.Template.TryMatch(existing.ToString(), out IssueKeyContext matched) &&
                        matched.Uid.Equals(uid, StringComparison.OrdinalIgnoreCase))
                    {
                        key = existing;
                        return true;
                    }
                }
            }

            key = IssueKey.None;
            return false;
        }

        /// <summary>
        ///     Tries to read substitution values from a key or path
        /// </summary>
        /// <param name="value">the key or path</param>
        /// <param name="context">the parsed values</param>
        /// <returns>true when the value matches the template</returns>
        public bool TryGetContext(string value, out IssueKeyContext context)
        {
            return this.Template.TryMatch(value, out context);
        }

        /// <summary>
        ///     Finds issue directories by their stored files, including keys that no longer match the current template
        /// </summary>
        /// <returns>the existing issue keys and their relative paths</returns>
        public IReadOnlyList<(IssueKey Key, string RelativePath)> FindExisting()
        {
            List<(IssueKey Key, string RelativePath)> keys = new List<(IssueKey Key, string RelativePath)>();
            if (!Directory.Exists(this.Root.IssuesPath))
            {
                return keys;
            }

            foreach (string directory in Directory.EnumerateDirectories(this.Root.IssuesPath, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(this.Root.IssuesPath, directory);
                if (string.IsNullOrEmpty(relative) || (relative == "."))
                {
                    continue;
                }

                string keyValue = IssueKeyTemplate.NormalizeKey(relative);
                string fileName = Path.GetFileName(keyValue);
                if (!System.IO.File.Exists(Path.Combine(directory, $"{fileName}.json")) &&
                    !System.IO.File.Exists(Path.Combine(directory, $"{fileName}.md")))
                {
                    continue;
                }

                keys.Add((IssueKey.Create(keyValue), relative));
            }

            return keys;
        }

        private IEnumerable<IssueKey> FindAll()
        {
            List<IssueKey> keys = new List<IssueKey>();
            if (!Directory.Exists(this.Root.IssuesPath))
            {
                return keys;
            }

            foreach (string directory in Directory.EnumerateDirectories(this.Root.IssuesPath, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(this.Root.IssuesPath, directory);
                if (string.IsNullOrEmpty(relative) || (relative == "."))
                {
                    continue;
                }

                if (!this.TryGetKey(relative, out IssueKey key))
                {
                    continue;
                }

                string expected = this.GetIssuePath(key);
                if (!string.Equals(
                        relative.Replace('\\', Path.DirectorySeparatorChar),
                        expected.Replace('\\', Path.DirectorySeparatorChar),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                keys.Add(key);
            }

            return keys;
        }
    }
}
