using System;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Validates issue type identifiers used in configuration and issue keys
    /// </summary>
    public static class IssueTypeName
    {
        /// <summary>
        ///     Validates an issue type identifier without changing its spelling
        /// </summary>
        /// <param name="value">the issue type identifier</param>
        /// <returns>the validated identifier</returns>
        public static string Validate(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("An issue type name is required.", nameof(value));
            }

            foreach (char character in value)
            {
                if (!((character >= 'A' && character <= 'Z') ||
                      (character >= 'a' && character <= 'z') ||
                      (character >= '0' && character <= '9')))
                {
                    throw new ArgumentException(
                        $"Issue type '{value}' must contain only ASCII letters and digits.",
                        nameof(value));
                }
            }

            return value;
        }
    }
}
