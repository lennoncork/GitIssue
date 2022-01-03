using System;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using GitIssue.Issues;
using GitIssue.Values;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Factory class that creates new fields
    /// </summary>
    public class FieldFactory : IFieldFactory
    {
        private readonly Func<IField?> callback;
        private readonly Issue issue;
        private readonly FieldKey key;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldFactory" /> class
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="key">the field key</param>
        /// <param name="callback">the callback to invoke when the new field is created</param>
        public FieldFactory(Issue issue, FieldKey key, Func<IField?> callback)
        {
            this.callback = callback;
            this.issue = issue;
            this.key = key;
        }

        /// <inheritdoc />
        public void WithValue<T>(T value) where T : IValue
        {
            var field = callback?.Invoke();
            if (field is ValueField<T> valueField)
                valueField.Value = value;
        }

        /// <inheritdoc />
        public void WithArray<T>(T[] values) where T : IValue
        {
            var field = callback?.Invoke();
            if (field is ArrayField<T> arrayField) arrayField.Values = values;
        }

        /// <inheritdoc />
        public void WithField(IField updated)
        {
            var field = callback?.Invoke();
            if (field?.GetType() == updated.GetType())
            {

            }

        }
    }
}