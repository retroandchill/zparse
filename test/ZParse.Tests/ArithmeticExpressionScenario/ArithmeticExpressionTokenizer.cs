using System.Collections.Generic;
using ZParse.Model;
using ZParse.Parsers;

namespace ZParse.Tests.ArithmeticExpressionScenario;

internal class ArithmeticExpressionTokenizer
    : Tokenizer<ArithmeticExpressionToken, ArithmeticExpressionTokenizer.Enumerator>
{
    private readonly Dictionary<char, ArithmeticExpressionToken> _operators = new()
    {
        ['+'] = ArithmeticExpressionToken.Plus,
        ['-'] = ArithmeticExpressionToken.Minus,
        ['*'] = ArithmeticExpressionToken.Times,
        ['/'] = ArithmeticExpressionToken.Divide,
        ['('] = ArithmeticExpressionToken.LParen,
        [')'] = ArithmeticExpressionToken.RParen,
    };

    protected override Enumerator Tokenize(TextSpan span)
    {
        return new Enumerator(SkipWhiteSpace(span), _operators);
    }

    internal ref struct Enumerator : ITokenEnumerator<ArithmeticExpressionToken>
    {
        private readonly Dictionary<char, ArithmeticExpressionToken> _operators;
        private Result<char> _next;

        internal Enumerator(
            Result<char> next,
            Dictionary<char, ArithmeticExpressionToken> operators
        )
        {
            _next = next;
            _operators = operators;
        }

        public bool NextToken(out Result<ArithmeticExpressionToken> token)
        {
            if (!_next.HasValue)
            {
                token = default;
                return false;
            }

            bool found;
            var ch = _next.Value;
            if (ch is >= '0' and <= '9')
            {
                var integer = Numerics.Integer(_next.Location);
                _next = integer.Remainder.ConsumeChar();
                token = Result.Value(
                    ArithmeticExpressionToken.Number,
                    integer.Location,
                    integer.Remainder
                );
                found = true;
            }
            else if (_operators.TryGetValue(ch, out var charToken))
            {
                token = Result.Value(charToken, _next.Location, _next.Remainder);
                _next = _next.Remainder.ConsumeChar();
                found = true;
            }
            else
            {
                token = Result.Empty<ArithmeticExpressionToken>(
                    _next.Location,
                    ["number", "operator"]
                );
                found = false;
            }

            _next = SkipWhiteSpace(_next.Location);
            return found;
        }

        public void Dispose()
        {
            // No resources to dispose of
        }
    }
}
