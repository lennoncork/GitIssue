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
        /// <param name="root">the issue root</param>
        public FileIssue(IssueRoot root) : base(root)
        {
        }

        /// <summary>
        ///     Initializes a new instance of a <see cref="FileIssue" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the issue manager</param>
        public FileIssue(IssueRoot root, IDictionary<FieldKey, FieldInfo> fields) :
            base(root, fields)
        {
        }

        /// <summary>
        ///     Deletes an issue and all it's fields from disk.
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <returns></returns>
        public new static async Task<bool> DeleteAsync(IssueRoot issueRoot)
        {
            bool result = await JsonIssue.DeleteAsync(issueRoot);
            if (!result)
            {
                return false;
            }

            if (Directory.Exists(issueRoot.IssuePath))
            {
                Directory.Delete(issueRoot.IssuePath, true);
            }

            return true;
        }

        /// <summary>
        ///     Reads an issue from disk
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns></returns>
        public new static async Task<IIssue?> ReadAsync(IssueRoot root,
            IDictionary<FieldKey, FieldInfo> fields)
        {
            if (!Directory.Exists(root.IssuePath))
            {
                return null;
            }

            FileIssue issue = new FileIssue(root, fields);
            foreach (FieldKey key in fields.Keys)
            {
                IField valueField = await fields[key].ReadFieldAsync(issue, key);
                issue.fields[key] = valueField;
            }

            return issue;
        }

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            // Save the json file
            await base.SaveAsync();

            // Save each modified field
            foreach (IField field in this.Values)
            {
                if (this.modifiedFields.Contains(field.Key))
                {
                    await field.SaveAsync();
                    this.modifiedFields.Remove(field.Key);
                }
            }

            return true;
        }
    }
}