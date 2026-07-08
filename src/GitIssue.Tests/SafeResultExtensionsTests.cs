using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitIssue.Tests
{
    [TestFixture]
    public class SafeResultExtensionsTests
    {
        [TestFixture]
        public class WithSafeResult : SafeResultExtensionsTests
        {
            [Test]
            public void GenericTask_Failure_Returns_Failed_SafeResult_With_Exception()
            {
                Exception ex = new InvalidOperationException("failure");
                Task<string> task = Task.FromException<string>(ex);

                SafeResult<string> result = task.WithSafeResult();

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.SameAs(ex));
            }

            [Test]
            public void GenericTask_Success_Returns_Successful_SafeResult_With_Result()
            {
                Task<string> task = Task.FromResult("ok");

                SafeResult<string> result = task.WithSafeResult();

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Exception, Is.Null);
                Assert.That(result.Result, Is.EqualTo("ok"));
            }

            [Test]
            public void Task_Failure_Returns_Failed_SafeResult_With_Exception()
            {
                Exception ex = new InvalidOperationException("failure");
                Task task = Task.FromException(ex);

                SafeResult result = task.WithSafeResult();

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.SameAs(ex));
            }

            [Test]
            public void Task_Success_Returns_Successful_SafeResult()
            {
                Task task = Task.CompletedTask;

                SafeResult result = task.WithSafeResult();

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Exception, Is.Null);
            }
        }

        [TestFixture]
        public class WithSafeResultAsyncTask : SafeResultExtensionsTests
        {
            [Test]
            public async Task Task_Failure_Returns_Failed_SafeResult_With_Exception()
            {
                Exception ex = new InvalidOperationException("failure");
                Task task = Task.FromException(ex);

                SafeResult result = await task.WithSafeResultAsync();

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.SameAs(ex));
            }

            [Test]
            public async Task Task_Success_Returns_Successful_SafeResult()
            {
                Task task = Task.CompletedTask;

                SafeResult result = await task.WithSafeResultAsync();

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Exception, Is.Null);
            }
        }
    }
}