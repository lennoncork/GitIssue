﻿using System;
using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;

namespace GitIssue.Formatters
{
    /// <summary>
    /// Extension methods for formatting the issue
    /// </summary>
    public static class FormattingExtensions
    {
        /// <summary>
        /// Formats the field using the provided formatter
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <returns></returns>
        public static string Format(this IReadOnlyIssue issue) => SimpleFormatter.Default.Format(issue);

        /// <summary>
        /// Formats the field using the provided formatter
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="formatter">the field formatter</param>
        /// <returns></returns>
        public static string Format(this IReadOnlyIssue issue, IIssueFormatter formatter) => formatter.Format(issue);

        /// <summary>
        /// Formats the field using the provided formatter
        /// </summary>
        /// <param name="field">the issue field</param>
        /// <param name="formatter">the field formatter</param>
        /// <returns></returns>
        public static string Format(this IField field, IFieldFormatter formatter) => formatter.Format(field);
    }
}
