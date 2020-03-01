using System;
using GitIssue.Formatters;
using NUnit.Framework;

namespace GitIssue.Tests.Formatting
{
    [TestFixture]
    public partial class DetailedFormatterTests : TestsBase
    {
        private Lazy<DetailedFormatter> formatter = new Lazy<DetailedFormatter>(() => new DetailedFormatter());

        public DetailedFormatter Sut => formatter.Value;

    }
}