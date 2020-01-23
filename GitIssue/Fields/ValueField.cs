using System;
using System.Threading.Tasks;

namespace GitIssue.Fields
{
    /// <summary>
    /// An issue field with a single value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueField<T> : Field
    {
        private readonly Func<T, string> formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueField{T}"/> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected ValueField(FieldKey key, T value) : this(key, value, v => v.ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueField{T}"/> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="formatter"></param>
        protected ValueField(FieldKey key, T value, Func<T, string> formatter) : base(key)
        {
            this.Value = value;
            this.formatter = formatter;
        }

        /// <summary>
        /// The field value
        /// </summary>
        public T Value { get; set; }

        /// <inheritdoc/>
        public override string ToString() => this.formatter.Invoke(this.Value);
    }
}
