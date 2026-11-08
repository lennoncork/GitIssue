using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Validation;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue.Issues.Markdown
{
    /// <summary>
    ///     Reads and writes an issue as one Markdown document
    /// </summary>
    public static class MarkdownIssueFormat
    {
        private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UsePreciseSourceLocation()
            .Build();

        /// <summary>
        ///     Reads issue fields from a Markdown document
        /// </summary>
        /// <param name="markdown">the document text</param>
        /// <param name="issueKey">the issue key taken from the file path</param>
        /// <param name="schema">the configured fields</param>
        /// <returns>the parsed values and validation errors</returns>
        public static MarkdownRead Read(string markdown, IssueKey issueKey, IDictionary<FieldKey, FieldInfo> schema)
        {
            string text = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            MarkdownDocument document = Markdig.Markdown.Parse(text, MarkdownIssueFormat.pipeline);
            List<IssueValidationError> errors = new List<IssueValidationError>();
            Dictionary<FieldKey, object?> values = new Dictionary<FieldKey, object?>();
            HashSet<FieldKey> seen = new HashSet<FieldKey>();
            OpenSection? open = null;

            foreach (Block block in document)
            {
                if (block is HeadingBlock heading && (heading.Level <= 2))
                {
                    MarkdownIssueFormat.close(text, issueKey, schema, open, heading.Span.Start, values, errors);
                    open = null;
                    if (heading.Level == 1)
                    {
                        string title = MarkdownIssueFormat.inlineText(heading.Inline).Trim();
                        if (!string.Equals(title, issueKey.ToString(), StringComparison.Ordinal))
                        {
                            errors.Add(new IssueValidationError(
                                issueKey,
                                FieldKey.Create("Key"),
                                heading.Line + 1,
                                $"{MarkdownIssueFormat.quote(title)} does not match issue key '{issueKey}'."));
                        }

                        continue;
                    }

                    string name = MarkdownIssueFormat.inlineText(heading.Inline).Trim();
                    FieldKey key = FieldKey.Create(name);
                    int line = heading.Line + 1;
                    if (!schema.ContainsKey(key))
                    {
                        errors.Add(new IssueValidationError(issueKey, key, line, "unknown field."));
                        open = OpenSection.Discarded(heading.Span.End + 1);
                        continue;
                    }

                    if (!seen.Add(key))
                    {
                        errors.Add(new IssueValidationError(issueKey, key, line, "duplicate field."));
                        open = OpenSection.Discarded(heading.Span.End + 1);
                        continue;
                    }

                    open = new OpenSection(key, line, heading.Span.End + 1);
                    continue;
                }

                if (open == null)
                {
                    if (!MarkdownIssueFormat.isBlank(block))
                    {
                        errors.Add(new IssueValidationError(
                            issueKey,
                            FieldKey.None,
                            block.Line + 1,
                            "content outside a field."));
                    }

                    continue;
                }

                if (!open.Discard)
                {
                    open.Blocks.Add(block);
                }
            }

            MarkdownIssueFormat.close(text, issueKey, schema, open, text.Length, values, errors);

            foreach (KeyValuePair<FieldKey, FieldInfo> field in schema)
            {
                if (field.Value.Required && !seen.Contains(field.Key))
                {
                    errors.Add(new IssueValidationError(issueKey, field.Key, null, "required field is missing."));
                }
            }

            return new MarkdownRead(errors, values);
        }

        /// <summary>
        ///     Writes an issue as a canonical Markdown document
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="schema">the configured fields</param>
        /// <returns>the Markdown document</returns>
        public static string Write(IIssue issue, IDictionary<FieldKey, FieldInfo> schema)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("# ").Append(issue.Key).Append('\n');
            foreach (KeyValuePair<FieldKey, FieldInfo> field in schema)
            {
                builder.Append('\n').Append("## ").Append(field.Key).Append("\n\n");
                if (!issue.TryGetValue(field.Key, out IField? value) || (value == null))
                {
                    continue;
                }

                builder.Append(MarkdownIssueFormat.formatField(field.Value, value));
                if ((builder.Length > 0) && (builder[^1] != '\n'))
                {
                    builder.Append('\n');
                }
            }

            return builder.ToString();
        }

        private static void close(
            string text,
            IssueKey issueKey,
            IDictionary<FieldKey, FieldInfo> schema,
            OpenSection? open,
            int contentEnd,
            Dictionary<FieldKey, object?> values,
            List<IssueValidationError> errors)
        {
            if ((open == null) || open.Discard || (open.Key == null))
            {
                return;
            }

            FieldKey key = open.Key.Value;
            FieldInfo info = schema[key];
            if (IssueValidator.IsTextArray(info))
            {
                values[key] = MarkdownIssueFormat.readComments(text, issueKey, key, info, open, contentEnd, errors);
                return;
            }

            if (IssueValidator.IsArray(info))
            {
                values[key] = MarkdownIssueFormat.readList(issueKey, key, info, open, errors);
                return;
            }

            string raw = MarkdownIssueFormat.slice(text, open.ContentStart, contentEnd);
            if (IssueValidator.TryAccept(info, key, raw, out object? parsed, out string? error))
            {
                if ((parsed is IssueKey parsedKey) && (key == FieldKey.Create("Key")) && (parsedKey != issueKey))
                {
                    errors.Add(new IssueValidationError(
                        issueKey,
                        key,
                        open.Line,
                        $"{MarkdownIssueFormat.quote(raw)} does not match issue key '{issueKey}'."));
                    return;
                }

                values[key] = parsed;
                return;
            }

            errors.Add(new IssueValidationError(issueKey, key, open.Line, error ?? "value is not valid."));
        }

        private static string formatField(FieldInfo info, IField field)
        {
            if (field is IArrayField arrayField)
            {
                object[] items = arrayField.Values ?? Array.Empty<object>();
                StringBuilder builder = new StringBuilder();
                if (IssueValidator.IsTextArray(info))
                {
                    for (int index = 0; index < items.Length; index++)
                    {
                        if (index > 0)
                        {
                            builder.Append('\n');
                        }

                        builder.Append("### ").Append(index + 1).Append("\n\n");
                        builder.Append(MarkdownIssueFormat.formatScalar(items[index]));
                        builder.Append('\n');
                    }

                    return builder.ToString();
                }

                foreach (object item in items)
                {
                    builder.Append("- ").Append(MarkdownIssueFormat.formatScalar(item).Replace("\n", " ", StringComparison.Ordinal)).Append('\n');
                }

                return builder.ToString();
            }

            if (field is IValueField valueField)
            {
                string text = MarkdownIssueFormat.formatScalar(valueField.Value);
                return string.IsNullOrEmpty(text) ? string.Empty : text + "\n";
            }

            return string.Empty;
        }

        private static string formatScalar(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime dateTime)
            {
                return dateTime.Item.ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is String text)
            {
                return text.Item ?? string.Empty;
            }

            return value.ToString() ?? string.Empty;
        }

        private static string inlineText(Inline? inline)
        {
            StringBuilder builder = new StringBuilder();
            MarkdownIssueFormat.appendInline(builder, inline);
            return builder.ToString();
        }

        private static void appendInline(StringBuilder builder, Inline? inline)
        {
            for (Inline? current = inline; current != null; current = current.NextSibling)
            {
                if (current is LiteralInline literal)
                {
                    builder.Append(literal.Content.ToString());
                }
                else if (current is CodeInline code)
                {
                    builder.Append(code.Content);
                }
                else if (current is LineBreakInline)
                {
                    builder.Append('\n');
                }
                else if (current is ContainerInline container)
                {
                    MarkdownIssueFormat.appendInline(builder, container.FirstChild);
                }
            }
        }

        private static string blockText(Block block)
        {
            StringBuilder builder = new StringBuilder();
            MarkdownIssueFormat.appendBlock(builder, block);
            return builder.ToString().Trim();
        }

        private static void appendBlock(StringBuilder builder, Block block)
        {
            if (block is LeafBlock leaf)
            {
                MarkdownIssueFormat.appendInline(builder, leaf.Inline);
                return;
            }

            if (block is ContainerBlock container)
            {
                bool first = true;
                foreach (Block child in container)
                {
                    if (!first)
                    {
                        builder.Append('\n');
                    }

                    first = false;
                    MarkdownIssueFormat.appendBlock(builder, child);
                }
            }
        }

        private static bool isBlank(Block block)
        {
            if (block is ThematicBreakBlock)
            {
                return false;
            }

            if (block is CodeBlock code)
            {
                return code.Lines.Count == 0;
            }

            if (block is HtmlBlock html)
            {
                return html.Lines.Count == 0;
            }

            return string.IsNullOrWhiteSpace(MarkdownIssueFormat.blockText(block));
        }

        private static string quote(string raw)
        {
            string single = raw.Replace("\n", "\\n", StringComparison.Ordinal);
            if (single.Length > 80)
            {
                single = single.Substring(0, 77) + "...";
            }

            return $"'{single}'";
        }

        private static object[] readComments(
            string text,
            IssueKey issueKey,
            FieldKey key,
            FieldInfo info,
            OpenSection open,
            int contentEnd,
            List<IssueValidationError> errors)
        {
            List<object> comments = new List<object>();
            HeadingBlock? current = null;
            foreach (Block block in open.Blocks)
            {
                if (block is HeadingBlock heading && (heading.Level == 3))
                {
                    MarkdownIssueFormat.addComment(text, issueKey, key, info, current, heading.Span.Start, open.Line, comments, errors);
                    current = heading;
                    continue;
                }

                if ((current == null) && !MarkdownIssueFormat.isBlank(block))
                {
                    errors.Add(new IssueValidationError(issueKey, key, block.Line + 1, "content is not a comment block."));
                }
            }

            MarkdownIssueFormat.addComment(text, issueKey, key, info, current, contentEnd, open.Line, comments, errors);
            return comments.ToArray();
        }

        private static void addComment(
            string text,
            IssueKey issueKey,
            FieldKey key,
            FieldInfo info,
            HeadingBlock? heading,
            int end,
            int fallbackLine,
            List<object> comments,
            List<IssueValidationError> errors)
        {
            if (heading == null)
            {
                return;
            }

            string raw = MarkdownIssueFormat.slice(text, heading.Span.End + 1, end);
            int line = heading.Line + 1;
            if (line <= 0)
            {
                line = fallbackLine;
            }

            if (!IssueValidator.TryAccept(info, key, raw, out object? parsed, out string? error))
            {
                errors.Add(new IssueValidationError(issueKey, key, line, error ?? "value is not valid."));
                return;
            }

            if (parsed != null)
            {
                comments.Add(parsed);
            }
        }

        private static object[] readList(
            IssueKey issueKey,
            FieldKey key,
            FieldInfo info,
            OpenSection open,
            List<IssueValidationError> errors)
        {
            List<object> items = new List<object>();
            foreach (Block block in open.Blocks)
            {
                if (block is ListBlock list)
                {
                    foreach (Block child in list)
                    {
                        if (child is not ListItemBlock item)
                        {
                            continue;
                        }

                        string raw = MarkdownIssueFormat.blockText(item);
                        if (!IssueValidator.TryAccept(info, key, raw, out object? parsed, out string? error))
                        {
                            errors.Add(new IssueValidationError(issueKey, key, item.Line + 1, error ?? "value is not valid."));
                            continue;
                        }

                        if (parsed != null)
                        {
                            items.Add(parsed);
                        }
                    }

                    continue;
                }

                if (!MarkdownIssueFormat.isBlank(block))
                {
                    errors.Add(new IssueValidationError(issueKey, key, block.Line + 1, "content is not a list."));
                }
            }

            return items.ToArray();
        }

        private static string slice(string text, int start, int end)
        {
            if (start < 0)
            {
                start = 0;
            }

            if (end < start)
            {
                end = start;
            }

            if (start > text.Length)
            {
                start = text.Length;
            }

            if (end > text.Length)
            {
                end = text.Length;
            }

            return text.Substring(start, end - start).Trim('\n');
        }

        private sealed class OpenSection
        {
            public OpenSection(FieldKey key, int line, int contentStart)
            {
                this.Key = key;
                this.Line = line;
                this.ContentStart = contentStart;
            }

            private OpenSection(int contentStart)
            {
                this.ContentStart = contentStart;
                this.Discard = true;
            }

            public List<Block> Blocks { get; } = new List<Block>();

            public int ContentStart { get; }

            public bool Discard { get; }

            public FieldKey? Key { get; }

            public int Line { get; }

            public static OpenSection Discarded(int contentStart)
            {
                return new OpenSection(contentStart);
            }
        }
    }
}