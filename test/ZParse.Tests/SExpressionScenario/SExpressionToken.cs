using ZParse.Display;
using ZParse.Model;

namespace ZParse.Tests.SExpressionScenario
{
    enum SExpressionToken
    {
        None,
        Atom,
        Number,

        [Token(Description = "open parenthesis")]
        LParen,

        [Token(Description = "closing parenthesis")]
        RParen
    }
}