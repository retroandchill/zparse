using ZParse.Display;
using ZParse.Model;

namespace ZParse.Tests.ComplexTokenScenario
{
    enum SExpressionType
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
