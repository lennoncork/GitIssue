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
        /// <param name="key"></param>
        /// <param name="value"></param>
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

        /// <summary>
        ///     Tries to parse the string input to the output value
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="value">the output value</param>
        /// <returns></returns>
        internal static bool TryParse(string input, out T value)
        {
            return ValueExtensions.TryParse(input, out value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}