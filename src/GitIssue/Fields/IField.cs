﻿using System.Threading.Tasks;
using GitIssue.Keys;
using Newtonsoft.Json.Linq;

namespace GitIssue.Fields
{
    /// <summary>
    /// Field Interface
    /// </summary>
    public interface IField
    {
        /// <summary>
        /// Gets the field's key
        /// </summary>
        FieldKey Key { get; }

        /// <summary>
        /// Saves any additional filed data
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveAsync();
    }
}
