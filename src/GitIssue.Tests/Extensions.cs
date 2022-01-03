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
            if (safe.IsSuccess == false && safe.Exception != null)
            {
                TestContext.Out.Write(safe.Exception);
            }
            Assert.That(safe.IsSuccess, Is.True, $"SafeResult was not successful, {safe.Exception?.Message}", safe);
            return safe.Result;
        }
    }
}
