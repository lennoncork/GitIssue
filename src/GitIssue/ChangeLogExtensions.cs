using System.Collections.Generic;
using System.Text;
using GitIssue.Issues;

namespace GitIssue
{
    /// <summary>
    ///     Extension methods for the change log
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
            StringBuilder builder = new StringBuilder();
            int count = 0;
            foreach (KeyValuePair<IssueKey, List<string>> changes in log.Log)
            {
                if (count++ > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine($"Issue: {changes.Key}");
                foreach (string change in changes.Value)
                {
                    builder.AppendLine($" - {change}");
                }
            }

            return builder.ToString();
        }
    }
}