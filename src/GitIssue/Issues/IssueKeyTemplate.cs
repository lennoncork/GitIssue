using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Parses and applies issue key templates such as {type}-{created}-{index}-{uid}-{slug}
    /// </summary>
    public sealed class IssueKeyTemplate
    {
        /// <summary>
        ///     Default template. The first directory is the issue type with a trailing s, then the date folders.
        /// </summary>
        public const string Default = "{type}s/{created:yyyy}/{created:MM}/{created:dd}/{uid}";

        /// <summary>
        ///     Date-based template matching the historical year/month/day folder layout
        /// </summary>
        public const string Date = "{created:yyyy}/{created:MM}/{created:dd}/{uid}";

        /// <summary>
        ///     Named template matching "UID - Issue Name"
        /// </summary>
        public const string Name = "{uid} - {name}";

        /// <summary>
        ///     Key stored inside a free-layout document. The folder is not part of the key.
        /// </summary>
        public const string Identity = "{type}-{uid}";

        private static readonly HashSet<char> InvalidFileNameCharacters = Path.GetInvalidFileNameChars()
            .Concat(new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' })
            .ToHashSet();

        private readonly List<Segment> segments;

        private IssueKeyTemplate(string source, List<Segment> segments)
        {
            this.Source = source;
            this.segments = segments;
        }

        /// <summary>
        ///     Gets the original template string
        /// </summary>
        public string Source { get; }

        /// <summary>
        ///     Gets a value indicating whether the template includes {index}
        /// </summary>
        public bool HasIndex => this.segments.Any(segment => segment.Token == Token.Index);

        /// <summary>
        ///     Gets a value indicating whether the template includes {uid}
        /// </summary>
        public bool HasUid => this.segments.Any(segment => segment.Token == Token.Uid);

        /// <summary>
        ///     Gets a value indicating whether the template includes {type}
        /// </summary>
        public bool HasType => this.segments.Any(segment => segment.Token == Token.Type);

        /// <summary>
        ///     Parses a key template
        /// </summary>
        /// <param name="template">the template string</param>
        /// <returns>the parsed template</returns>
        public static IssueKeyTemplate Parse(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("A key template is required.", nameof(template));
            }

            List<Segment> segments = new List<Segment>();
            int index = 0;
            while (index < template.Length)
            {
                int tokenStart = template.IndexOf('{', index);
                if (tokenStart < 0)
                {
                    segments.Add(Segment.CreateLiteral(template.Substring(index)));
                    break;
                }

                if (tokenStart > index)
                {
                    segments.Add(Segment.CreateLiteral(template.Substring(index, tokenStart - index)));
                }

                int tokenEnd = template.IndexOf('}', tokenStart);
                if (tokenEnd < 0)
                {
                    throw new ArgumentException($"Key template '{template}' has an unterminated token.", nameof(template));
                }

                string body = template.Substring(tokenStart + 1, tokenEnd - tokenStart - 1);
                int formatSeparator = body.IndexOf(':');
                string name = formatSeparator < 0 ? body : body.Substring(0, formatSeparator);
                string format = formatSeparator < 0 ? string.Empty : body.Substring(formatSeparator + 1);
                segments.Add(Segment.FromToken(name, format, template, segments.Count));
                index = tokenEnd + 1;
            }

            if (segments.Count == 0)
            {
                throw new ArgumentException("A key template is required.", nameof(template));
            }

            return new IssueKeyTemplate(template, segments);
        }

        /// <summary>
        ///     Converts a path or key to the canonical key form
        /// </summary>
        /// <param name="value">the key or path</param>
        /// <returns>the normalized key</returns>
        public static string NormalizeKey(string value)
        {
            return value.Trim().Replace('\\', '-').Replace('/', '-');
        }

        /// <summary>
        ///     Sanitizes a display name for use in a key
        /// </summary>
        /// <param name="name">the issue name</param>
        /// <returns>the sanitized name</returns>
        public static string SanitizeName(string name)
        {
            return IssueKeyTemplate.sanitize(name, slug: false);
        }

        /// <summary>
        ///     Sanitizes a name as a lowercase slug
        /// </summary>
        /// <param name="name">the issue name</param>
        /// <returns>the slug</returns>
        public static string SanitizeSlug(string name)
        {
            return IssueKeyTemplate.sanitize(name, slug: true);
        }

        private static string sanitize(string value, bool slug)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return slug ? "untitled" : "Untitled";
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool pendingSeparator = false;
            foreach (char character in value.Trim())
            {
                if (IssueKeyTemplate.InvalidFileNameCharacters.Contains(character) || char.IsControl(character))
                {
                    pendingSeparator = builder.Length > 0;
                    continue;
                }

                if (char.IsWhiteSpace(character) || (slug && (character == '-')))
                {
                    pendingSeparator = builder.Length > 0;
                    continue;
                }

                if (pendingSeparator)
                {
                    builder.Append(slug ? '-' : ' ');
                    pendingSeparator = false;
                }

                builder.Append(slug ? char.ToLowerInvariant(character) : character);
            }

            string sanitized = builder.ToString().TrimEnd('.', ' ', '-');
            string fallback = slug ? "untitled" : "Untitled";
            if (string.IsNullOrEmpty(sanitized) || (sanitized == ".") || (sanitized == ".."))
            {
                return fallback;
            }

            const int maximumLength = 80;
            if (sanitized.Length > maximumLength)
            {
                sanitized = sanitized.Substring(0, maximumLength).TrimEnd('.', ' ', '-');
            }

            return string.IsNullOrEmpty(sanitized) ? fallback : sanitized;
        }

        /// <summary>
        ///     Renders the unique issue key, using '-' in place of template path separators
        /// </summary>
        /// <param name="context">the substitution values</param>
        /// <returns>the issue key</returns>
        public string RenderKey(IssueKeyContext context)
        {
            return this.render(context, key: true);
        }

        /// <summary>
        ///     Renders the relative issue path, using directory separators from the template
        /// </summary>
        /// <param name="context">the substitution values</param>
        /// <returns>the relative issue path</returns>
        public string RenderPath(IssueKeyContext context)
        {
            return this.render(context, key: false);
        }

        /// <summary>
        ///     Tries to read substitution values from an existing key or path
        /// </summary>
        /// <param name="value">the key or relative path</param>
        /// <param name="context">the parsed values</param>
        /// <returns>true when the value matches this template</returns>
        public bool TryMatch(string value, out IssueKeyContext context)
        {
            context = new IssueKeyContext();
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalized = IssueKeyTemplate.NormalizeKey(value);
            Match match = this.buildKeyRegex().Match(normalized);
            if (!match.Success)
            {
                return false;
            }

            string type = "Issue";
            string uid = string.Empty;
            string name = "Untitled";
            int index = 0;
            List<string> createdFormats = new List<string>();
            List<string> createdValues = new List<string>();

            foreach (Segment segment in this.segments)
            {
                if (segment.Token == Token.Literal)
                {
                    continue;
                }

                string group = match.Groups[segment.GroupName].Value;
                switch (segment.Token)
                {
                    case Token.Type:
                        type = group;
                        break;
                    case Token.Uid:
                        uid = group;
                        break;
                    case Token.Name:
                        name = group;
                        break;
                    case Token.Slug:
                        name = group.Replace('-', ' ');
                        break;
                    case Token.Index:
                        int.TryParse(group, NumberStyles.Integer, CultureInfo.InvariantCulture, out index);
                        break;
                    case Token.Created:
                        createdFormats.Add(string.IsNullOrEmpty(segment.Format) ? "yyyy-MM-dd" : segment.Format);
                        createdValues.Add(group);
                        break;
                }
            }

            DateTime created = DateTime.Now;
            if (createdFormats.Count > 0)
            {
                string combinedFormat = string.Concat(createdFormats);
                string combinedValue = string.Concat(createdValues);
                if (!DateTime.TryParseExact(combinedValue, combinedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out created))
                {
                    return false;
                }
            }

            context = new IssueKeyContext
            {
                Type = type,
                Created = created,
                Index = index,
                Uid = uid,
                Name = name,
            };
            return true;
        }

        private string render(IssueKeyContext context, bool key)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Segment segment in this.segments)
            {
                if (segment.Token == Token.Literal)
                {
                    builder.Append(key ? segment.Literal.Replace('/', '-').Replace('\\', '-') : segment.Literal.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
                    continue;
                }

                builder.Append(this.renderToken(segment, context));
            }

            return builder.ToString();
        }

        private string renderToken(Segment segment, IssueKeyContext context)
        {
            switch (segment.Token)
            {
                case Token.Type:
                    return IssueTypeName.Validate(context.Type);
                case Token.Uid:
                    return string.IsNullOrEmpty(context.Uid) ? string.Empty : context.Uid.ToUpperInvariant();
                case Token.Name:
                    return IssueKeyTemplate.SanitizeName(context.Name);
                case Token.Slug:
                    return IssueKeyTemplate.SanitizeSlug(context.Name);
                case Token.Index:
                    int index = context.Index <= 0 ? 1 : context.Index;
                    return string.IsNullOrEmpty(segment.Format)
                        ? index.ToString(CultureInfo.InvariantCulture)
                        : index.ToString(segment.Format, CultureInfo.InvariantCulture);
                case Token.Created:
                    string format = string.IsNullOrEmpty(segment.Format) ? "yyyy-MM-dd" : segment.Format;
                    return context.Created.ToString(format, CultureInfo.InvariantCulture);
                default:
                    return string.Empty;
            }
        }

        private Regex buildKeyRegex()
        {
            StringBuilder builder = new StringBuilder("^");
            foreach (Segment segment in this.segments)
            {
                if (segment.Token == Token.Literal)
                {
                    builder.Append(Regex.Escape(segment.Literal.Replace('/', '-').Replace('\\', '-')));
                    continue;
                }

                builder.Append("(?<").Append(segment.GroupName).Append('>').Append(this.tokenPattern(segment)).Append(')');
            }

            builder.Append('$');
            return new Regex(builder.ToString(), RegexOptions.CultureInvariant);
        }

        private string tokenPattern(Segment segment)
        {
            switch (segment.Token)
            {
                case Token.Uid:
                    return $"[A-Fa-f0-9]{{{IssueKeyProvider.UniqueIdLength}}}";
                case Token.Index:
                    return @"\d+";
                case Token.Created:
                    return this.createdPattern(string.IsNullOrEmpty(segment.Format) ? "yyyy-MM-dd" : segment.Format);
                case Token.Type:
                    return @"[A-Za-z0-9]+";
                case Token.Slug:
                    return @"[^/\\]+";
                case Token.Name:
                    return @".+";
                default:
                    return ".+";
            }
        }

        private string createdPattern(string format)
        {
            StringBuilder builder = new StringBuilder();
            int index = 0;
            while (index < format.Length)
            {
                if (format.AsSpan(index).StartsWith("yyyy", StringComparison.Ordinal) ||
                    format.AsSpan(index).StartsWith("YYYY", StringComparison.Ordinal))
                {
                    builder.Append(@"\d{4}");
                    index += 4;
                    continue;
                }

                if (format.AsSpan(index).StartsWith("MM", StringComparison.Ordinal) ||
                    format.AsSpan(index).StartsWith("dd", StringComparison.Ordinal) ||
                    format.AsSpan(index).StartsWith("HH", StringComparison.Ordinal) ||
                    format.AsSpan(index).StartsWith("mm", StringComparison.Ordinal) ||
                    format.AsSpan(index).StartsWith("ss", StringComparison.Ordinal))
                {
                    builder.Append(@"\d{2}");
                    index += 2;
                    continue;
                }

                builder.Append(Regex.Escape(format[index].ToString()));
                index++;
            }

            return builder.ToString();
        }

        private enum Token
        {
            Literal,
            Type,
            Created,
            Index,
            Uid,
            Name,
            Slug,
        }

        private readonly struct Segment
        {
            private Segment(Token token, string literal, string format, string groupName)
            {
                this.Token = token;
                this.Literal = literal;
                this.Format = format;
                this.GroupName = groupName;
            }

            public Token Token { get; }

            public string Literal { get; }

            public string Format { get; }

            public string GroupName { get; }

            public static Segment CreateLiteral(string value)
            {
                return new Segment(Token.Literal, value, string.Empty, string.Empty);
            }

            public static Segment FromToken(string name, string format, string template, int index)
            {
                Token token = name.ToLowerInvariant() switch
                {
                    "type" => Token.Type,
                    "created" => Token.Created,
                    "index" => Token.Index,
                    "uid" => Token.Uid,
                    "name" => Token.Name,
                    "title" => Token.Name,
                    "slug" => Token.Slug,
                    _ => throw new ArgumentException($"Key template '{template}' contains unknown token '{{{name}}}'.", nameof(template)),
                };

                return new Segment(token, string.Empty, format, $"{token.ToString().ToLowerInvariant()}_{index}");
            }
        }
    }
}
