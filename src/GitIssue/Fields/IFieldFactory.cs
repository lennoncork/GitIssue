﻿using GitIssue.Values;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Provides an interface for a factory that generates fields from values
    /// </summary>
    public interface IFieldFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        void WithField(IField field);

        /// <summary>
        ///     Updates a field using the specified value
        /// </summary>
        /// <typeparam name="T">the field data type</typeparam>
        /// <param name="value">the field value</param>
        /// <returns>A new field that fulfills the <see cref="IField" /> interface</returns>
        void WithValue<T>(T value) where T : IValue;

        /// <summary>
        ///     Updates the field with the specified array of values
        /// </summary>
        /// <typeparam name="T">the field data type</typeparam>
        /// <param name="values">the array of values</param>
        /// <returns>A new field that fulfills the <see cref="IField" /> interface</returns>
        void WithArray<T>(T[] values) where T : IValue;
    }
}