﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Values;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Issue Interface
    /// </summary>
    public interface IIssue : IReadOnlyIssue
    {
        /// <summary>
        ///     Gets or sets the issue title
        /// </summary>
        new String Title { get; set; }

        /// <summary>
        ///     Gets or sets the issue description
        /// </summary>
        new String Description { get; set; }

        /// <summary>
        /// Gets or sets the Author
        /// </summary>
        new Signature Author { get; set; }

        /// <summary>
        ///     Gets or sets when the issue was created
        /// </summary>
        new DateTime Created { get; set; }

        /// <summary>
        ///     Gets or sets when the issue was last updates
        /// </summary>
        new DateTime Updated { get; set; }

        /// <summary>
        ///     Gets a <see cref="IFieldFactory" /> for the provided key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFieldFactory SetField([CallerMemberName] string? key = null);

        /// <summary>
        ///     Gets a <see cref="IFieldFactory" /> for the provided <see cref="FieldKey" />
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFieldFactory SetField(FieldKey key);

        /// <summary>
        ///     Saves the issue
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveAsync();
    }
}