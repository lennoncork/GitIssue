using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Keys;

namespace GitIssue
{
    /// <summary>
    /// Issue Interface
    /// </summary>
    public interface IReadOnlyIssue : IReadOnlyDictionary<FieldKey, IField>
    {
        /// <summary>
        /// Gets the Issue Key
        /// </summary>
        IssueKey Key { get; }

        /// <summary>
        /// Gets or sets the issue title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets or sets the issue description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets or sets when the issue was created
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Gets or sets when the issue was last updates
        /// </summary>
        DateTime Updated { get; }

        /// <summary>
        /// Gets a <see cref="IFieldProvider"/> for a provided key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFieldProvider GetField([CallerMemberName] string key = null);

        /// <summary>
        /// Gets a <see cref="IFieldProvider"/> for the provided <see cref="FieldKey"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFieldProvider GetField(FieldKey key);
    }
}