using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using GitIssue.Values;
using NUnit.Framework;
using DateTime = System.DateTime;

namespace GitIssue.Tests.Formatting
{
    [TestFixture]
    public partial class DetailedFormatterTests
    {
        [TestFixture]
        public class Format : DetailedFormatterTests
        {
            [TestCaseSource(typeof(FormatTestCases))]
            public string FormatsIssue(IReadOnlyIssue issue)
            {
                return issue.Format(this.Sut);
            }

            public class FormatTestCases : IEnumerable<TestCaseData>
            {
                public IssueKey Key { get; set; }

                public IssueKey NextKey
                {
                    get
                    {
                        this.Key = Moqs.CreateIssueKey();
                        return this.Key;
                    }
                }

                public DateTime DateTime { get; set; }

                public DateTime NextDateTime
                {
                    get
                    {
                        this.DateTime = DateTime.Now;
                        return this.DateTime;
                    }
                }

                public IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Moqs.CreateIssue(NextKey,"Format", "", 
                            NextDateTime, DateTime))
                        .Returns($"Key: {Key}" + Environment.NewLine + 
                                 $"Title: Format" + Environment.NewLine +
                                 $"Description: " + Environment.NewLine +
                                 $"Created: {DateTime.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine +
                                 $"Updated: {DateTime.ToString(CultureInfo.InvariantCulture)}");
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }
        }
    }
}