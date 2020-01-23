using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues
{
    /// <summary>
    /// Represents an issue saved to disk
    /// </summary>
    public class FileIssue : JsonIssue
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="FileIssue"/> class
        /// </summary>
        /// <param name="issueRoot"></param>
        public FileIssue(IssueRoot issueRoot) : base(issueRoot)
        {
           
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="FileIssue"/> class
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <param name="fields"></param>
        public FileIssue(IssueRoot issueRoot, IDictionary<FieldKey, FieldInfo> fields) : base(issueRoot, fields)
        {
            
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveAsync()
        {
            // Save the json file
            await base.SaveAsync();

            // Save each modified field
            foreach (var field in Values)
            {
                if (this.modifiedFields.Contains(field.Key))
                {
                    await field.SaveAsync();
                    this.modifiedFields.Remove(field.Key);
                }
            }

            return true;
        }

        /// <summary>
        /// Reads an issue from disk
        /// </summary>
        /// <param name="issueRoot">the issue root</param>
        /// <param name="fields">the expected fields</param>
        /// <returns></returns>
        public new static async Task<IIssue> ReadAsync(IssueRoot issueRoot, IDictionary<FieldKey, FieldInfo> fields)
        {
            if (Directory.Exists(issueRoot.IssuePath) == false)
                return null;

            FileIssue issue = new FileIssue(issueRoot, fields);
            foreach (var key in fields.Keys)
            {
                var valueField = await fields[key].ReadFieldAsync(issue, key);
                issue.fields[key] = valueField;
            }

            return issue;
        }

        /// <summary>
        /// Deletes an issue and all it's fields from disk. 
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <returns></returns>
        public new static async Task DeleteAsync(IssueRoot issueRoot)
        {
            await JsonIssue.DeleteAsync(issueRoot);

            if(Directory.Exists(issueRoot.IssuePath))
                Directory.Delete(issueRoot.IssuePath, true);
        }
    }
}