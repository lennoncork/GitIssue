﻿namespace GitIssue.Util
{
    /// <summary>
    ///     The GitIssueCommand
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        ///     No command, default
        /// </summary>
        None,

        /// <summary>
        ///     Initialize the issues
        /// </summary>
        Init,

        /// <summary>
        ///     Creates a new issue
        /// </summary>
        Create,

        /// <summary>
        /// Commits the issues
        /// </summary>
        Commit,

        /// <summary>
        ///     Deletes and existing issue
        /// </summary>
        Delete,

        /// <summary>
        ///     Deletes and existing issue
        /// </summary>
        Track,

        /// <summary>
        ///     Finds an existing issue
        /// </summary>
        Find,

        /// <summary>
        ///     Shows the details of an issue
        /// </summary>
        Show,

        /// <summary>
        ///     Edits an existing issue
        /// </summary>
        Edit,

        /// <summary>
        ///     Shows the fields available for use
        /// </summary>
        Fields,

        /// <summary>
        /// Adds a value to an existing field
        /// </summary>
        Add,

        /// <summary>
        /// Removes an existing value from an existing field
        /// </summary>
        Remove
    }
}