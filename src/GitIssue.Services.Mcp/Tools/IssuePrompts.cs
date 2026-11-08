using System.ComponentModel;
using ModelContextProtocol.Server;

namespace GitIssue.Services.Mcp.Tools
{
    /// <summary>
    ///     Prompt templates for working with GitIssue
    /// </summary>
    [McpServerPromptType]
    public static class IssuePrompts
    {
        /// <summary>
        ///     Asks the model to summarize the repository issues
        /// </summary>
        /// <returns>the prompt text</returns>
        [McpServerPrompt(Name = "triage-issues")]
        [Description("Summarize repository issues against the configured fields.")]
        public static string TriageIssues()
        {
            return "Read the gitissue://schema resource and call list_issues. Summarize each issue with its key, title, and the field values that distinguish features from other work. Do not invent fields that list_fields does not return.";
        }

        /// <summary>
        ///     Asks the model to draft an issue from the schema
        /// </summary>
        /// <returns>the prompt text</returns>
        [McpServerPrompt(Name = "draft-issue")]
        [Description("Draft a title, description, and legal field values for a new issue.")]
        public static string DraftIssue()
        {
            return "Call list_fields, then draft a title, description, and values only for fields that exist. Use +value when proposing an array field such as Labels. Do not commit the issue.";
        }
    }
}
