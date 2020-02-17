using System;
using System.Threading.Tasks;
using GitIssue.Values;

namespace GitIssue.Fields.Value
{
    /// <summary>
    ///     An issue field with a single value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueField<T> : Field, IValueField<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueField{T}" /> class
        /// </summary>
        /// <param name="key">the issue key</param>
        /// <param name="value">the issue value</param>
        protected ValueField(FieldKey key, T value) : base(key)
        {
            Value = value;
        }

        /// <inheritdoc />
        public Type ValueType => typeof(T);

        /// <inheritdoc />
        public T Value { get; set; }

        /// <inheritdoc />
        public override Task<bool> UpdateAsync(string input)
        {
            if (TryParse(input, out var result))
            {
                Value = result;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        bool IValueField.TryParse(string input, out object? value)
        {
            if (ValueExtensions.TryParse(input, out T result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryParse(string input, out T value)
        {
            return ValueExtensions.TryParse(input, out value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value?.ToString()!;
        }
    }
}