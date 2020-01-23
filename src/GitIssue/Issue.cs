using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue
{
    /// <summary>
    /// Abstract issue implementation
    /// </summary>
    public abstract class Issue : IIssue
    {
        /// <summary>
        /// Creates a new Issue
        /// </summary>
        /// <param name="root">the issue root</param>
        protected Issue(IssueRoot root)
        {
            this.Root = root;
        }

        /// <summary>
        /// Gets or sets the repository root
        /// </summary>
        public IssueRoot Root { get; protected set; }

        /// <inheritdoc/>
        public IssueKey Key => this.Root.Key;

        /// <inheritdoc/>
        public string Title
        {
            get => this.GetField().AsValue<string>();
            set => this.SetField().WithValue<string>(value);
        }

        /// <inheritdoc/>
        public string Description
        {
            get => this.GetField().AsValue<string>();
            set => this.SetField().WithValue<string>(value);
        }

        /// <inheritdoc/>
        public DateTime Created
        {
            get => this.GetField().AsValue<DateTime>();
            set => this.SetField().WithValue<DateTime>(value);
        }

        /// <inheritdoc/>
        public DateTime Updated
        {
            get => this.GetField().AsValue<DateTime>();
            set => this.SetField().WithValue<DateTime>(value);
        }

        /// <inheritdoc/>
        public abstract IFieldProvider GetField([CallerMemberName] string key = null);

        /// <inheritdoc/>
        public abstract IFieldProvider GetField(FieldKey key);

        /// <inheritdoc/>
        public abstract IFieldFactory SetField([CallerMemberName] string key = null);

        /// <inheritdoc/>
        public abstract IFieldFactory SetField(FieldKey key);

        /// <inheritdoc/>
        public abstract IEnumerable<FieldKey> Keys { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<IField> Values { get; }

        /// <inheritdoc/>
        public abstract int Count { get; }

        /// <inheritdoc/>
        public abstract IField this[FieldKey key] { get; }

        /// <inheritdoc/>
        public abstract Task<bool> SaveAsync();

        /// <inheritdoc/>
        public abstract bool ContainsKey(FieldKey key);

        /// <inheritdoc/>
        public abstract bool TryGetValue(FieldKey key, [MaybeNullWhen(false)] out IField value);

        /// <inheritdoc/>
        public abstract IEnumerator<KeyValuePair<FieldKey, IField>> GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
