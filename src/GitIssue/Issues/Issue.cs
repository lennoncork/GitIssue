﻿using System.Collections;
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

        /// <summary>
        ///     Gets or sets the repository root
        /// </summary>
        public IssueRoot Root { get; }

        /// <inheritdoc />
        public IssueKey Key
        {
            get => GetField().AsValue<IssueKey>();
            protected set => SetField().WithValue<IssueKey>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public String Title
        {
            get => GetField().AsValue<String>();
            set => SetField().WithValue<String>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public String Description
        {
            get => GetField().AsValue<String>();
            set => SetField().WithValue<String>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public Signature Author
        {
            get => GetField().AsValue<Signature>();
            set => SetField().WithValue<Signature>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public DateTime Created
        {
            get => GetField().AsValue<DateTime>();
            set => SetField().WithValue<DateTime>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public DateTime Updated
        {
            get => GetField().AsValue<DateTime>();
            set => SetField().WithValue<DateTime>(value);
        }

        /// <inheritdoc cref="IIssue" />
        public String[] Comments
        {
            get => GetField().AsArray<String>();
            set => SetField().WithArray<String>(value);
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
            return $"{Key} {Title}";
        }

        /// <inheritdoc />
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            FieldKey key = FieldKey.Create(binder.Name);
            if (TryGetValue(key, out var field))
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
            if (TryGetValue(key, out var field))
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
    }
}