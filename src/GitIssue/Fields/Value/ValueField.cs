using System;
using System.Diagnostics.CodeAnalysis;
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
            this.Value = value;
        }

        /// <inheritdoc />
        public T Value { get; set; }

        /// <inheritdoc />
        public Type ValueType => typeof(T);

        object? IValueField.Value
        {
            get => this.Value;
            set
            {
                if (value is T result)
                {
                    this.Value = result;
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Value?.ToString()!;
        }

        /// <inheritdoc />
        public bool TryParse(string input, out T value)
        {
            return ValueExtensions.TryParse(input, out value);
        }

        /// <inheritdoc />
        public override bool Equals([AllowNull] IField other)
        {
            if (other is IValueField<T> valueField)
            {
                return this.Value?.Equals(valueField.Value) ?? false;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Copy([AllowNull] IField other)
        {
            if (other is IValueField<T> valueField)
            {
                this.Value = valueField.Value;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Update(string input)
        {
            if (this.TryParse(input, out T result))
            {
                this.Value = result;
                return true;
            }

            return false;
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

        bool IValueField<T>.TryParse(string input, out T value)
        {
            throw new NotImplementedException();
        }
    }
}