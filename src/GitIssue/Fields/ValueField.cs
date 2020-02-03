using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitIssue.Keys;

namespace GitIssue.Fields
{
    /// <summary>
    ///     An issue field with a single value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueField<T> : Field
    {
        private readonly Func<T, string> formatter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueField{T}" /> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected ValueField(FieldKey key, T value) : this(key, value, v => v.ToString())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueField{T}" /> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="formatter"></param>
        protected ValueField(FieldKey key, T value, Func<T, string> formatter) : base(key)
        {
            Value = value;
            this.formatter = formatter;
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
        internal static bool TryParse(string input, out T value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                value = (T)converter.ConvertFrom(input);
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return formatter.Invoke(Value);
        }
    }
}