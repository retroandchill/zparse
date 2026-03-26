using ZParse.Parsers;
using ZParse.Tests.Support;
using Xunit;

namespace ZParse.Tests.Combinators
{
    public class ManyDelimitedByCombinatorTests
    {
        [Fact]
        public void AnEndDelimiterCanBeSpecified()
        {
            AssertParser.SucceedsWith(
                Token.EqualTo('a').Value('a')
                    .ManyDelimitedBy(Token.EqualTo('b'), end: Token.EqualTo('c')),
                "ababac",
                new[] {'a', 'a', 'a'});
        }
    }
}