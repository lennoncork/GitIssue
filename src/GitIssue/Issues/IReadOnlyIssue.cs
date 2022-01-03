using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GitIssue.Fields;
using GitIssue.Values;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Issue Interface
    /// </summary>
    public interface IReadOnlyIssue : IReadOnlyDictionary<FieldKey, IField>
    {
        /// <summary>
        ///     Gets the Issue Key
        /// </summary>
        IssueKey Key { get; }

        /// <summary>
        ///     Gets or sets the issue title
        /// </summary>
        String Title { get; }

        /// <summary>
        ///     Gets or sets the issue description
        /// </summary>
        String Description { get; }

        /// <summary>
        ///     Gets or sets when the issue was created
        /// </summary>
        Signature Author { get; }

        /// <summary>
        ///     Gets or sets when the issue was created
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        ///     Gets or sets when the issue was last updates
        /// </summary>
        DateTime Updated { get; }

        /// <summary>
        ///     Gets a <see cref="IFieldProvider" /> for a provided key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFieldProvider GetField([CallerMemberName] string? key = null);

        /// <summary>
        ///     Gets a <see cref="IFieldProvider" /> for the provided <see cref="FieldKey" />
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFieldProvider GetField(FieldKey key);
    }
}