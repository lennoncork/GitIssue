﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitIssue.Tests
{
    public static class Extensions
    {
        /// <summary>
        /// Asserts if the result wasn't successful, otherwise returns the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task<T> AssertIfNotSuccess<T>(this Task<SafeResult<T>> result)
        {
            var safe = await result;
            Assert.That(safe.IsSuccess, Is.True);
            return safe.Result;
        }
    }
}
