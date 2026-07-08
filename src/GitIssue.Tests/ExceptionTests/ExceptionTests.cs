using System;
using GitIssue.Exceptions;
using NUnit.Framework;

namespace GitIssue.Tests.ExceptionTests
{
    [TestFixture]
    public class ExceptionTests
    {
        [TestFixture]
        public class SafeResult : ExceptionTests
        {
            [Test]
            public void SafeResultException_Sets_Message_And_InnerException()
            {
                Exception inner = new InvalidOperationException("inner");

                SafeResultException ex1 = new SafeResultException("message");
                SafeResultException ex2 = new SafeResultException("message2", inner);

                Assert.That(ex1.Message, Is.EqualTo("message"));
                Assert.That(ex1.InnerException, Is.Null);

                Assert.That(ex2.Message, Is.EqualTo("message2"));
                Assert.That(ex2.InnerException, Is.SameAs(inner));
            }
        }

        [TestFixture]
        public class IssueNotFound : ExceptionTests
        {
            [Test]
            public void IssueNotFoundException_Sets_Message_And_InnerException()
            {
                Exception inner = new Exception("inner");

                IssueNotFoundException ex1 = new IssueNotFoundException("msg1");
                IssueNotFoundException ex2 = new IssueNotFoundException("msg2", inner);

                Assert.That(ex1.Message, Is.EqualTo("msg1"));
                Assert.That(ex1.InnerException, Is.Null);

                Assert.That(ex2.Message, Is.EqualTo("msg2"));
                Assert.That(ex2.InnerException, Is.SameAs(inner));
            }
        }

        [TestFixture]
        public class RepositoryNotFound : ExceptionTests
        {
            [Test]
            public void RepositoryNotFoundException_Sets_Message_And_InnerException()
            {
                Exception inner = new Exception("inner");

                RepositoryNotFoundException ex1 = new RepositoryNotFoundException("msg1");
                RepositoryNotFoundException ex2 = new RepositoryNotFoundException("msg2", inner);

                Assert.That(ex1.Message, Is.EqualTo("msg1"));
                Assert.That(ex1.InnerException, Is.Null);

                Assert.That(ex2.Message, Is.EqualTo("msg2"));
                Assert.That(ex2.InnerException, Is.SameAs(inner));
            }
        }
    }
}