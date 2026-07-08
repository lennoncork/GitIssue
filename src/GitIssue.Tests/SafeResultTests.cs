using System;
using NUnit.Framework;

namespace GitIssue.Tests
{
    [TestFixture]
    public class SafeResultTests
    {
        [TestFixture]
        public class NonGeneric : SafeResultTests
        {
            [Test]
            public void Fail_Returns_NotSuccessful_With_Null_Exception()
            {
                SafeResult result = SafeResult.Fail();

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.Null);
            }

            [Test]
            public void Fail_With_Exception_Sets_Exception_And_NotSuccessful()
            {
                Exception ex = new Exception("failure");

                SafeResult result = SafeResult.Fail(ex);

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.SameAs(ex));
            }

            [Test]
            public void Success_Returns_Successful_Result_With_Null_Exception()
            {
                SafeResult result = SafeResult.Success();

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Exception, Is.Null);
            }
        }

        [TestFixture]
        public class Generic : SafeResultTests
        {
            [Test]
            public void Fail_Generic_Has_Default_Result_And_NotSuccessful()
            {
                SafeResult<int> result = SafeResult<int>.Fail();

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.Null);
                Assert.That(result.Result, Is.EqualTo(default(int)));
            }

            [Test]
            public void Fail_Generic_With_Exception_Sets_Exception_And_NotSuccessful()
            {
                Exception ex = new Exception("failure");

                SafeResult<int> result = SafeResult<int>.Fail(ex);

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Exception, Is.SameAs(ex));
                Assert.That(result.Result, Is.EqualTo(default(int)));
            }

            [Test]
            public void GetResult_Returns_Default_When_NotSuccessful_And_NoThrow()
            {
                SafeResult<string> result = SafeResult<string>.Fail();

                string value = result.GetResult(false);

                Assert.That(value, Is.EqualTo(default(string)));
            }

            [Test]
            public void GetResult_Returns_Result_When_Successful()
            {
                SafeResult<string> result = SafeResult<string>.Success("ok");

                string value = result.GetResult();

                Assert.That(value, Is.EqualTo("ok"));
            }

            [Test]
            public void GetResult_Throws_SafeResultException_When_NotSuccessful_And_No_Exception()
            {
                SafeResult<string> result = SafeResult<string>.Fail();

                Assert.That(() => result.GetResult(), Throws.TypeOf<SafeResultException>());
            }

            [Test]
            public void GetResult_Throws_SafeResultException_With_Inner_Exception_When_Present()
            {
                Exception inner = new Exception("failure");
                SafeResult<string> result = SafeResult<string>.Fail(inner);

                SafeResultException exception = Assert.Throws<SafeResultException>(() => result.GetResult())!;

                Assert.That(exception.InnerException, Is.SameAs(inner));
            }

            [Test]
            public void HasResult_Returns_False_And_Default_Result_When_NotSuccessful()
            {
                SafeResult<string> result = SafeResult<string>.Fail();

                bool hasResult = result.HasResult(out string value);

                Assert.That(hasResult, Is.False);
                Assert.That(value, Is.EqualTo(default(string)));
            }

            [Test]
            public void HasResult_Returns_True_And_Sets_Out_Result_When_Successful()
            {
                SafeResult<string> result = SafeResult<string>.Success("ok");

                bool hasResult = result.HasResult(out string value);

                Assert.That(hasResult, Is.True);
                Assert.That(value, Is.EqualTo("ok"));
            }

            [Test]
            public void Success_With_Result_Sets_IsSuccess_And_Result()
            {
                SafeResult<string> result = SafeResult<string>.Success("value");

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Exception, Is.Null);
                Assert.That(result.Result, Is.EqualTo("value"));
            }
        }
    }
}