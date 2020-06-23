using System;
using System.Diagnostics.CodeAnalysis;
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

        object? IValueField.Value 
        { 
            get => Value;
            set
            {
                if (value is T result)
                {
                    Value = result;
                }
            }
        }

        /// <inheritdoc />
        public override bool Update(string input)
        {
            if (TryParse(input, out var result))
            {
                Value = result;
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

        bool IValueField<T>.TryParse(string input, out T value)
        {
            throw new NotImplementedException();
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
        public override bool Equals([AllowNull] IField other)
        {
            if (other is IValueField<T> valueField)
            {
                return this.Value?.Equals(valueField.Value) ?? false;
            }
            return false;
        }
    }
}