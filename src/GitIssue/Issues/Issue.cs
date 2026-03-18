using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Values;
using DateTime = GitIssue.Values.DateTime;
using String = GitIssue.Values.String;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Abstract issue implementation
    /// </summary>
    public abstract class Issue : DynamicObject, IIssue
    {
        /// <summary>
        ///     Creates a new Issue
        /// </summary>
        /// <param name="root">the issue root</param>
        protected Issue(IssueRoot root)
        {
            this.Root = root;
        }

        /// <inheritdoc cref="IIssue" />
        public Signature Author
        {
            get => this.GetField().AsValue<Signature>();
            set => this.SetField().WithValue(value);
        }

        /// <inheritdoc cref="IIssue" />
        public String[] Comments
        {
            get => this.GetField().AsArray<String>();
            set => this.SetField().WithArray(value);
        }

        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc cref="IIssue" />
        public DateTime Created
        {
            get => this.GetField().AsValue<DateTime>();
            set => this.SetField().WithValue(value);
        }

        /// <inheritdoc cref="IIssue" />
        public String Description
        {
            get => this.GetField().AsValue<String>();
            set => this.SetField().WithValue(value);
        }

        /// <inheritdoc />
        public abstract IField this[FieldKey key] { get; }

        /// <inheritdoc />
        public IssueKey Key
        {
            get => this.GetField().AsValue<IssueKey>();
            protected set => this.SetField().WithValue(value);
        }

        /// <inheritdoc />
        public abstract IEnumerable<FieldKey> Keys { get; }

        /// <summary>
        ///     Gets or sets the repository root
        /// </summary>
        public IssueRoot Root { get; }

        /// <inheritdoc cref="IIssue" />
        public String Title
        {
            get => this.GetField().AsValue<String>();
            set => this.SetField().WithValue(value);
        }

        /// <inheritdoc cref="IIssue" />
        public DateTime Updated
        {
            get => this.GetField().AsValue<DateTime>();
            set => this.SetField().WithValue(value);
        }

        /// <inheritdoc />
        public abstract IEnumerable<IField> Values { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Key} {this.Title}";
        }

        /// <inheritdoc />
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            FieldKey key = FieldKey.Create(binder.Name);
            if (this.TryGetValue(key, out IField? field))
            {
                result = field;
                return true;
            }

            result = null!;
            return true;
        }

        /// <inheritdoc />
        public override bool TrySetMember(
            SetMemberBinder binder, object? value)
        {
            FieldKey key = FieldKey.Create(binder.Name);
            if (this.TryGetValue(key, out IField? field))
            {
                if (field is IValueField valueField)
                {
                    if (value?.GetType() == valueField.ValueType)
                    {
                        valueField.Value = value;
                    }
                }
                else if (field is IArrayField arrayField)
                {
                    if (value is IEnumerable enumerable)
                    {
                        arrayField.Values = enumerable.Cast<object>().ToArray();
                    }
                }
                else if (value?.GetType() == typeof(string))
                {
                    if (field.Update((string)value))
                    {
                        return true;
                    }
                }
                else
                {
                    if (field.Update(value?.ToString() ?? string.Empty))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public abstract IEnumerator<KeyValuePair<FieldKey, IField>> GetEnumerator();

        /// <inheritdoc />
        public abstract Task<bool> SaveAsync();

        /// <inheritdoc />
        public abstract IFieldFactory SetField([CallerMemberName] string? key = null);

        /// <inheritdoc />
        public abstract IFieldFactory SetField(FieldKey key);

        /// <inheritdoc />
        public abstract bool ContainsKey(FieldKey key);

        /// <inheritdoc />
        public abstract bool TryGetValue(FieldKey key, [MaybeNullWhen(false)] out IField value);

        /// <inheritdoc />
        public abstract IFieldProvider GetField([CallerMemberName] string? key = null);

        /// <inheritdoc />
        public abstract IFieldProvider GetField(FieldKey key);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}