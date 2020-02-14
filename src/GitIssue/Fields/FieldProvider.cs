using System;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Issues;
using GitIssue.Values;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Provider class that extracts values from fields
    /// </summary>
    public class FieldProvider : IFieldProvider
    {
        private readonly Func<IField?> callback;
        private readonly Issue issue;
        private readonly FieldKey key;

        /// <summary>
        ///     Initialized a new instance of the <see cref="FieldProvider" /> class
        /// </summary>
        /// <param name="issue">the parent issue</param>
        /// <param name="key">the field key</param>
        /// <param name="callback">the callback to invoke to get the field</param>
        public FieldProvider(Issue issue, FieldKey key, Func<IField?> callback)
        {
            this.issue = issue;
            this.key = key;
            this.callback = callback;
        }

        /// <inheritdoc />
        public T AsValue<T>() where T : IValue
        {
            if (callback?.Invoke() is ValueField<T> fileField) return fileField.Value;
            return default!;
        }

        /// <inheritdoc />
        public T[] AsArray<T>() where T : IValue
        {
            if (callback?.Invoke() is ArrayField<T> fileField) return fileField.Values;
            return null!;
        }
    }
}