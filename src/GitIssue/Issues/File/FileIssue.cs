using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues.Json;

namespace GitIssue.Issues.File
{
    /// <summary>
    ///     Represents an issue saved to disk
    /// </summary>
    public class FileIssue : JsonIssue
    {
        /// <summary>
        ///     Initializes a new instance of a <see cref="FileIssue" /> class
        /// </summary>
        /// <param name="manager">the issue manager</param>
        /// <param name="root">the issue root</param>
        public FileIssue(IIssueManager manager, IssueRoot root) : base(manager, root)
        {
        }

        /// <summary>
        ///     Initializes a new instance of a <see cref="FileIssue" /> class
        /// </summary>
        /// <param name="manager">the issue manager</param>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the issue manager</param>
        public FileIssue(IIssueManager manager, IssueRoot root, IDictionary<FieldKey, FieldInfo> fields) : 
            base(manager, root, fields)
        {
            
        }

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            // Save the json file
            await base.SaveAsync();

            // Save each modified field
            foreach (var field in Values)
                if (modifiedFields.Contains(field.Key))
                {
                    await field.SaveAsync();
                    modifiedFields.Remove(field.Key);
                }

            return true;
        }

        /// <summary>
        ///     Reads an issue from disk
        /// </summary>
        /// <param name="manager">the issue manager</param>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns></returns>
        public new static async Task<IIssue?> ReadAsync(IIssueManager manager, IssueRoot root, IDictionary<FieldKey, FieldInfo> fields)
        {
            if (Directory.Exists(root.IssuePath) == false)
                return null;

            var issue = new FileIssue(manager, root, fields);
            foreach (var key in fields.Keys)
            {
                var valueField = await fields[key].ReadFieldAsync(issue, key);
                issue.fields[key] = valueField;
            }

            return issue;
        }

        /// <summary>
        ///     Deletes an issue and all it's fields from disk.
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <returns></returns>
        public new static async Task DeleteAsync(IssueRoot issueRoot)
        {
            await JsonIssue.DeleteAsync(issueRoot);

            if (Directory.Exists(issueRoot.IssuePath))
                Directory.Delete(issueRoot.IssuePath, true);
        }
    }
}