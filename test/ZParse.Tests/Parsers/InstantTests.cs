using ZParse.Parsers;
using ZParse.Tests.Support;
using Xunit;

namespace ZParse.Tests.Parsers
{
    public class InstantTests
    {
        [Theory]
        [InlineData("0", false)]
        [InlineData("1910-10-28T03:04:05", true)]
        [InlineData("2020-10-28T03:04:05", true)]
        [InlineData("1910-10-28T03:04:05.6789", true)]
        [InlineData("1910-10-28T03:04:05Z", true)]
        [InlineData("1910-10-28T03:04:05+10:00", true)]
        [InlineData("1910-10-28T03:04:05-07:30", true)]
        // A number of cases allowed by the spec aren't yet covered, here.
        public void IsoDateTimesAreRecognized(string input, bool isMatch)
        {
            AssertParser.FitsTheory(Instant.Iso8601DateTime, input, isMatch);
        }        
    }
}