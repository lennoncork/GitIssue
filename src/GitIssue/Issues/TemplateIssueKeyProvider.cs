using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using GitIssue.Issues.Markdown;
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

        private static IEnumerable<string> enumerateDirectories(string root)
        {
            if (!Directory.Exists(root))
            {
                yield break;
            }

            Queue<string> pending = new Queue<string>();
            pending.Enqueue(root);
            while (pending.Count > 0)
            {
                string current = pending.Dequeue();
                IEnumerable<string> children;
                try
                {
                    children = Directory.EnumerateDirectories(current);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (string child in children)
                {
                    if (Path.GetFileName(child).StartsWith(".", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    yield return child;
                    pending.Enqueue(child);
                }
            }
        }

        private static IEnumerable<string> enumerateFiles(string root)
        {
            if (!Directory.Exists(root))
            {
                yield break;
            }

            foreach (string file in TemplateIssueKeyProvider.safeFiles(root))
            {
                yield return file;
            }

            foreach (string directory in TemplateIssueKeyProvider.enumerateDirectories(root))
            {
                foreach (string file in TemplateIssueKeyProvider.safeFiles(directory))
                {
                    yield return file;
                }
            }
        }

        private static IEnumerable<string> safeFiles(string directory)
        {
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directory);
            }
            catch (IOException)
            {
                yield break;
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }

            foreach (string file in files)
            {
                if (!Path.GetFileName(file).StartsWith(".", StringComparison.Ordinal))
                {
                    yield return file;
                }
            }
        }

        private static string? readContentKey(string file)
        {
            string text;
            try
            {
                text = System.IO.File.ReadAllText(file);
            }
            catch (IOException)
            {
                return null;
            }

            if (Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                return MarkdownIssueFormat.ReadKey(text);
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(text);
                if ((document.RootElement.ValueKind != JsonValueKind.Object) ||
                    !document.RootElement.TryGetProperty("Key", out JsonElement key) ||
                    (key.ValueKind != JsonValueKind.String))
                {
                    return null;
                }

                return key.GetString();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private readonly IIssueConfiguration configuration;

        private readonly string fallbackTemplate;

        private readonly Dictionary<string, (string Directory, string? FileName)> placed =
            new Dictionary<string, (string Directory, string? FileName)>(StringComparer.Ordinal);

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
                bool foreignPreset = (this.fallbackTemplate == IssueKeyTemplate.Name) &&
                    ((this.configuration.KeyTemplate == IssueKeyTemplate.Date) ||
                     (this.configuration.KeyTemplate == IssueKeyTemplate.Default));
                if (string.IsNullOrWhiteSpace(this.configuration.KeyTemplate) || foreignPreset)
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
            (string Directory, string? FileName)? located = this.locate(key);
            if (located != null)
            {
                return located.Value.Directory;
            }

            if (!this.Template.TryMatch(key.ToString(), out IssueKeyContext context))
            {
                return key.ToString();
            }

            string fullPath = this.Template.RenderPath(context);
            if (Directory.Exists(Path.Combine(this.Root.IssuesPath, fullPath)))
            {
                return fullPath;
            }

            if (this.configuration.Storage == IssueStorage.Markdown)
            {
                return this.Template.RenderDirectory(context);
            }

            return fullPath;
        }

        /// <inheritdoc />
        public override string? GetDocumentFileName(IssueKey key)
        {
            return this.locate(key)?.FileName;
        }

        /// <summary>
        ///     Remembers where a new issue will be written before the file exists
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="directory">the directory relative to the issue root</param>
        /// <param name="fileName">the document file name</param>
        public void Place(IssueKey key, string directory, string? fileName)
        {
            this.placed[key.ToString()] = (directory, fileName);
        }

        /// <summary>
        ///     Finds issue documents by their key field, and legacy files named after their directory
        /// </summary>
        /// <returns>the documents</returns>
        public IReadOnlyList<(IssueKey Key, string Directory, string? FileName, bool FromContent)> FindDocuments()
        {
            List<(IssueKey Key, string Directory, string? FileName, bool FromContent)> documents =
                new List<(IssueKey Key, string Directory, string? FileName, bool FromContent)>();
            if (!Directory.Exists(this.Root.IssuesPath))
            {
                return documents;
            }

            Dictionary<string, string> seen = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string file in TemplateIssueKeyProvider.enumerateFiles(this.Root.IssuesPath))
            {
                string extension = Path.GetExtension(file);
                if (!extension.Equals(".md", StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relativeFile = Path.GetRelativePath(this.Root.IssuesPath, file);
                string? directory = Path.GetDirectoryName(relativeFile);
                if (string.IsNullOrEmpty(directory) || directory.Equals(".", StringComparison.Ordinal))
                {
                    directory = string.Empty;
                }

                string stem = Path.GetFileNameWithoutExtension(file);
                string pathKey = IssueKeyTemplate.NormalizeKey(directory);
                bool pathMatch = !string.IsNullOrEmpty(pathKey) &&
                    stem.Equals(pathKey, StringComparison.OrdinalIgnoreCase);
                string? contentKey = TemplateIssueKeyProvider.readContentKey(file);
                bool fromContent = !string.IsNullOrWhiteSpace(contentKey);
                string? keyValue = fromContent ? contentKey!.Trim() : pathMatch ? pathKey : null;
                if (string.IsNullOrWhiteSpace(keyValue))
                {
                    continue;
                }

                if (seen.TryGetValue(keyValue, out string? existing))
                {
                    string existingDirectory = (Path.GetDirectoryName(existing) ?? string.Empty).Replace('\\', '/');
                    string currentDirectory = directory.Replace('\\', '/');
                    bool sameDocument = existingDirectory.Equals(currentDirectory, StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(existing).Equals(keyValue, StringComparison.OrdinalIgnoreCase) &&
                        stem.Equals(keyValue, StringComparison.OrdinalIgnoreCase);
                    if (sameDocument)
                    {
                        continue;
                    }

                    throw new InvalidOperationException(
                        $"Issue '{keyValue}' is stored in both '{existing}' and '{relativeFile.Replace('\\', '/')}'. ");
                }

                seen[keyValue] = relativeFile.Replace('\\', '/');
                bool namedAfterKey = stem.Equals(keyValue, StringComparison.OrdinalIgnoreCase);
                documents.Add((IssueKey.Create(keyValue), directory, namedAfterKey ? null : Path.GetFileName(file), fromContent));
            }

            return documents;
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
                    string? extracted = TemplateIssueKeyProvider.ExtractUid(existing.ToString());
                    if ((extracted != null) && extracted.Equals(uid, StringComparison.OrdinalIgnoreCase))
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
            return this.FindDocuments()
                .Select(document => (document.Key, document.Directory))
                .ToList();
        }

        private IEnumerable<IssueKey> FindAll()
        {
            List<IssueKey> keys = new List<IssueKey>();
            if (!Directory.Exists(this.Root.IssuesPath))
            {
                return keys;
            }

            foreach (string directory in TemplateIssueKeyProvider.enumerateDirectories(this.Root.IssuesPath))
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

            foreach ((IssueKey documentKey, string _, string? _, bool fromContent) in this.FindDocuments())
            {
                if (fromContent && !keys.Contains(documentKey))
                {
                    keys.Add(documentKey);
                }
            }

            return keys;
        }

        private (string Directory, string? FileName)? locate(IssueKey key)
        {
            foreach ((IssueKey existing, string directory, string? fileName, bool _) in this.FindDocuments())
            {
                if (existing == key)
                {
                    return (directory, fileName);
                }
            }

            if (this.placed.TryGetValue(key.ToString(), out (string Directory, string? FileName) placedDocument))
            {
                return placedDocument;
            }

            return null;
        }
    }
}
