using Xunit;
using ZParse.Model;
using ZParse.Parsers;

namespace ZParse.Tests.Parsers;

public class QuotedStringTests
{
    [Fact]
    public void SqlStyleStringsAreParsed()
    {
        var input = new TextSpan("'Hello, ''world''!'x");
        var parser = QuotedString.SqlStyle;
        var r = parser(input);
        Assert.Equal("Hello, 'world'!", r.Value);
    }

    [Fact]
    public void CStyleStringsAreParsed()
    {
        var input = new TextSpan("\"Hello, \\\"world\\\"!\"x");
        var parser = QuotedString.CStyle;
        var r = parser(input);
        Assert.Equal("Hello, \"world\"!", r.Value);
    }
}
