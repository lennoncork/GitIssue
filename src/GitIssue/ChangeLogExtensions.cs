using System;
using System.Collections.Generic;
using System.Text;

namespace GitIssue
{
    /// <summary>
    /// Extension methods for the change log
    /// </summary>
    public static class ChangeLogExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GenerateComments(this IChangeLog log)
        {
            var builder = new StringBuilder();
            var count = 0;
            foreach (var changes in log.Log)
            {
                if (count++ > 0) builder.AppendLine();
                builder.AppendLine($"Issue: {changes.Key}");
                foreach (var change in changes.Value) builder.AppendLine($" - {change}");
            }
            return builder.ToString();
        }
    }
}
