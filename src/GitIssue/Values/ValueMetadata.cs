using System;
using System.Collections.Generic;
using System.Text;

namespace GitIssue.Values
{
    /// <summary>
    /// Stores a value and it's metadata for conversion
    /// </summary>
    public struct ValueMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueMetadata"/> struct
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        public ValueMetadata(string value, string metadata)
        {
            this.Value = value;
            this.Metadata = metadata;
        }

        /// <summary>
        /// The value to convert
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The metadata to provide the converter (for validation)
        /// </summary>
        public string Metadata { get; }
    }
}
