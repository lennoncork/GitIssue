using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Keys;
using GitIssue.Values;

namespace GitIssue.Fields
{
    /// <summary>
    ///     An issue field with a single value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueField<T> : Field
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

        /// <summary>
        ///     The field value
        /// </summary>
        public T Value { get; set; }

        /// <inheritdoc />
        public override Task<bool> UpdateAsync(string input)
        {
            if (TryParse(input, out T result))
            {
                this.Value = result;
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
        internal static bool TryParse(string input, out T value) => ValueExtensions.TryParse<T>(input, out value);

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}