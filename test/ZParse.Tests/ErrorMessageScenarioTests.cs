using System.Collections.Generic;
using Xunit;
using ZParse.Model;
using ZParse.Parsers;
using ZParse.Tests.ArithmeticExpressionScenario;
using ZParse.Tests.ComplexTokenScenario;
using ZParse.Tests.SExpressionScenario;
using ZParse.Tests.Support;
using ZParse.Tokenizers;

namespace ZParse.Tests;

public class ErrorMessageScenarioTests
{
    [Fact]
    public void ErrorMessagesFromAppliedCharacterParsersPropagate()
    {
        var number = Token
            .EqualTo(SExpressionToken.Number)
            .Apply(_ => Character.EqualTo('1').Then(_ => Character.EqualTo('x')));

        var numbers = number.AtEnd();

        AssertParser.FailsWithMessage(
            numbers,
            "123",
            new SExpressionTokenizer(),
            "Syntax error (line 1, column 2): invalid number, unexpected `2`, expected `x`."
        );
    }

    [Fact]
    public void ErrorMessageFromMatchedTokenProducesMeaningfulError()
    {
        var number = Token.Matching<SExpressionXToken>(
            x => x.Number < 100,
            "number less than `100`"
        );

        var numbers = number.AtEnd();

        AssertParser.FailsWithMessage(
            numbers,
            "123",
            new SExpressionXTokenizer(),
            "Syntax error (line 1, column 1): unexpected S-Expression Token `123`, expected number less than `100`."
        );
    }

    [Fact]
    public void ErrorMessageFromPartialItemsPropagate()
    {
        var atom = Token.EqualTo(SExpressionToken.Atom);
        var number = Token.EqualTo(SExpressionToken.Number);

        var alternating = number.Then(_ => atom).AtEnd();

        AssertParser.FailsWithMessage(
            alternating,
            "123 123",
            new SExpressionTokenizer(),
            "Syntax error (line 1, column 5): unexpected number `123`, expected atom."
        );
    }

    [Fact]
    public void ErrorMessageFromLastPartialItemPropagates()
    {
        var atom = Token.EqualTo(SExpressionToken.Atom);
        var number = Token.EqualTo(SExpressionToken.Number);

        var alternating = number.Then(_ => atom).Many().AtEnd();

        AssertParser.FailsWithMessage(
            alternating,
            "123 abc 123 123",
            new SExpressionTokenizer(),
            "Syntax error (line 1, column 13): unexpected number `123`, expected atom."
        );
    }

    [Fact]
    public void ErrorMessageFromIncompleteItemPropagates()
    {
        var atom = Token.EqualTo(SExpressionToken.Atom);
        var number = Token.EqualTo(SExpressionToken.Number);

        var alternating = number.Then(_ => atom).AtEnd();

        AssertParser.FailsWithMessage(
            alternating,
            "123",
            new SExpressionTokenizer(),
            "Syntax error: unexpected end of input, expected atom."
        );
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<object[]> ArithmeticExpressionTokenizers()
    {
        yield return [new ArithmeticExpressionTokenizer()];

        yield return
        [
            new TokenizerBuilder<ArithmeticExpressionToken>()
                .Ignore(Span.WhiteSpace)
                .Match(Character.EqualTo('+'), ArithmeticExpressionToken.Plus)
                .Match(Character.EqualTo('-'), ArithmeticExpressionToken.Minus)
                .Match(Character.EqualTo('*'), ArithmeticExpressionToken.Times)
                .Match(Character.EqualTo('/'), ArithmeticExpressionToken.Divide)
                .Match(Character.EqualTo('('), ArithmeticExpressionToken.LParen)
                .Match(Character.EqualTo(')'), ArithmeticExpressionToken.RParen)
                .Match(Numerics.Natural, ArithmeticExpressionToken.Number, requireDelimiters: true)
                .Build(),
        ];
    }

    [Theory, MemberData(nameof(ArithmeticExpressionTokenizers))]
    public void DroppedClosingParenthesisProducesMeaningfulError(
        ITokenizer<ArithmeticExpressionToken> tokenizer
    )
    {
        AssertParser.FailsWithMessage(
            ArithmeticExpressionParser.Lambda,
            "1 + (2 * 3",
            tokenizer,
            "Syntax error: unexpected end of input, expected `)`."
        );
    }

    [Theory, MemberData(nameof(ArithmeticExpressionTokenizers))]
    public void MissingOperandProducesMeaningfulError(
        ITokenizer<ArithmeticExpressionToken> tokenizer
    )
    {
        AssertParser.FailsWithMessage(
            ArithmeticExpressionParser.Lambda,
            "1 + * 3",
            tokenizer,
            "Syntax error (line 1, column 5): unexpected operator `*`, expected expression."
        );
    }

    [Theory, MemberData(nameof(ArithmeticExpressionTokenizers))]
    public void MissingOperatorProducesMeaningfulError(
        ITokenizer<ArithmeticExpressionToken> tokenizer
    )
    {
        AssertParser.FailsWithMessage(
            ArithmeticExpressionParser.Lambda,
            "1 3",
            tokenizer,
            "Syntax error (line 1, column 3): unexpected number `3`."
        );
    }

    [Fact]
    public void AmbiguousMatchesFailWithoutTry()
    {
        var abc = Span.EqualTo("ab").Or(Span.EqualTo("ac"));
        AssertParser.FailsWithMessage(
            abc,
            "ac",
            "Syntax error (line 1, column 2): unexpected `c`, expected `b`."
        );
    }

    [Fact]
    public void AmbiguousMatchesProducePreciseErrors()
    {
        var abc = Span.EqualTo("ab").Try().Or(Span.EqualTo("ac"));
        AssertParser.FailsWithMessage(
            abc,
            "bb",
            "Syntax error (line 1, column 1): unexpected `b`, expected `ab` or `ac`."
        );
    }

    [Fact]
    public void AmbiguousPrefixMatchesProducePreciseErrors()
    {
        var abc = Span.EqualTo("ab").Try().Or(Span.EqualTo("ac"));
        AssertParser.FailsWithMessage(
            abc,
            "ad",
            "Syntax error (line 1, column 2): unexpected `d`, expected `b` or `c`."
        );
    }

    [Fact]
    public void EmptySpanEqualToCharProducesCorrectExpectations()
    {
        var equalToA = Span.EqualTo('a');
        AssertParser.FailsWithMessage(
            equalToA,
            "",
            "Syntax error: unexpected end of input, expected `a`."
        );
    }

    [Fact]
    public void EmptySpanEqualToCharProducesCorrectExpectationsIgnoreCase()
    {
        var equalToA = Span.EqualToIgnoreCase('a');
        AssertParser.FailsWithMessage(
            equalToA,
            "",
            "Syntax error: unexpected end of input, expected `a`."
        );
    }

    [Fact]
    public void MessageWithExpectedTokensUsesTokenPresentation()
    {
        // Composing a complex parser which does not fit a LALR(1) grammar, one might need
        // to have multiple look-ahead tokens. While it is possible to compose parsers with back-tracking,
        // manual generated parsers are some times easier to construct. These parsers would like
        // to report expectations using tokens, but still benefit from the annotations put on
        // the tokens, to generated nicely formatted error messages. The following construct
        // shows how to generate an empty result, which indicates which tokens are expected.
        var emptyParseResult = TokenListParserResult.Empty<ArithmeticExpressionToken, string>(
            new TokenList<ArithmeticExpressionToken>(),
            [ArithmeticExpressionToken.Times, ArithmeticExpressionToken.Zero]
        );

        // Empty result represent expectations using nice string representation taken from
        // annotations of enum values of tokens
        Assert.Equal(2, emptyParseResult.Expectations.Length);
        Assert.Equal("`*`", emptyParseResult.Expectations[0]);
        Assert.Equal("`zero`", emptyParseResult.Expectations[1]);
    }
}
