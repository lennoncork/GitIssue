﻿using System.Threading.Tasks;

namespace GitIssue.Util
{
    /// <summary>
    ///     The GitIssueCommand
    /// </summary>
    public abstract class Command<T> where T : Options
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="options">the command options</param>
        /// <returns></returns>
        public abstract Task Exec(T options);
    }
}