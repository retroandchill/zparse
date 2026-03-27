using ZParse.Display;

namespace ZParse.Tests.SExpressionScenario;

internal enum SExpressionToken
{
    None,
    Atom,
    Number,

    [Token(Description = "open parenthesis")]
    LParen,

    [Token(Description = "closing parenthesis")]
    RParen
}