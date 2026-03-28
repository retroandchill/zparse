using System.Linq;
using ZParse.Model;

namespace ZParse.Tests.Support;

internal static class StringAsCharTokenList
{
    public static TokenList<char> Tokenize(string tokens)
    {
        var items = tokens
            .ToCharArray()
            .Select((ch, i) => new Token<char>(ch, new TextSpan(tokens, new Position(i, 1, 1), 1)))
            .ToArray();

        return new TokenList<char>(tokens, items);
    }
}
