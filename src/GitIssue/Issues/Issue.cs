using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Values;
using DateTime = System.DateTime;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Abstract issue implementation
    /// </summary>
    public abstract class Issue : IIssue
    {
        /// <summary>
        ///     The issue manager
        /// </summary>
        protected readonly IIssueManager manager;

        /// <summary>
        ///     Creates a new Issue
        /// </summary>
        /// <param name="manager">the issue manager</param>
        /// <param name="root">the issue root</param>
        protected Issue(IIssueManager manager, IssueRoot root)
        {
            this.manager = manager;
            Root = root;
        }

        /// <summary>
        ///     Gets or sets the repository root
        /// </summary>
        public IssueRoot Root { get; protected set; }

        /// <inheritdoc />
        public IssueKey Key => Root.Key;

        /// <inheritdoc cref="IIssue" />
        public string Title
        {
            get => GetField().AsValue<String>();
            set => SetField().WithValue<String>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public string Description
        {
            get => GetField().AsValue<String>();
            set => SetField().WithValue<String>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public DateTime Created
        {
            get => GetField().AsValue<Values.DateTime>();
            set => SetField().WithValue<Values.DateTime>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public DateTime Updated
        {
            get => GetField().AsValue<Values.DateTime>();
            set => SetField().WithValue<Values.DateTime>(value);
        }

        /// <inheritdoc />
        public abstract IFieldProvider GetField([CallerMemberName] string? key = null);

        /// <inheritdoc />
        public abstract IFieldProvider GetField(FieldKey key);

        /// <inheritdoc />
        public abstract IFieldFactory SetField([CallerMemberName] string? key = null);

        /// <inheritdoc />
        public abstract IFieldFactory SetField(FieldKey key);

        /// <inheritdoc />
        public abstract IEnumerable<FieldKey> Keys { get; }

        /// <inheritdoc />
        public abstract IEnumerable<IField> Values { get; }

        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc />
        public abstract IField this[FieldKey key] { get; }

        /// <inheritdoc />
        public abstract Task<bool> SaveAsync();

        /// <inheritdoc />
        public abstract bool ContainsKey(FieldKey key);

        /// <inheritdoc />
        public abstract bool TryGetValue(FieldKey key, [MaybeNullWhen(false)] out IField value);

        /// <inheritdoc />
        public abstract IEnumerator<KeyValuePair<FieldKey, IField>> GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Key.ToString()} {Title}";
        }
    }
}