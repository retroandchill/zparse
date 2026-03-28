using System.Collections.Generic;
using ZParse.Model;
using ZParse.Parsers;

namespace ZParse.Tests.ComplexTokenScenario;

internal class SExpressionXTokenizer
    : Tokenizer<SExpressionXToken, SExpressionXTokenizer.Enumerator>
{
    protected override Enumerator Tokenize(TextSpan span)
    {
        var next = SkipWhiteSpace(span);
        return new Enumerator(next);
    }

    public ref struct Enumerator : ITokenEnumerator<SExpressionXToken>
    {
        private Result<char> _next;

        internal Enumerator(Result<char> next)
        {
            _next = next;
        }

        public bool NextToken(out Result<SExpressionXToken> token)
        {
            if (!_next.HasValue)
            {
                token = default;
                return false;
            }

            if (_next.Value == '(')
            {
                token = Result.Value(
                    new SExpressionXToken(SExpressionType.LParen),
                    _next.Location,
                    _next.Remainder
                );
                _next = _next.Remainder.ConsumeChar();
            }
            else if (_next.Value == ')')
            {
                token = Result.Value(
                    new SExpressionXToken(SExpressionType.RParen),
                    _next.Location,
                    _next.Remainder
                );
                _next = _next.Remainder.ConsumeChar();
            }
            else if (_next.Value >= '0' && _next.Value <= '9')
            {
                var integer = Numerics.IntegerInt32(_next.Location);
                _next = integer.Remainder.ConsumeChar();

                token = Result.Value(
                    new SExpressionXToken(integer.Value),
                    integer.Location,
                    integer.Remainder
                );

                if (
                    _next.HasValue
                    && !char.IsPunctuation(_next.Value)
                    && !char.IsWhiteSpace(_next.Value)
                )
                {
                    token = Result.Empty<SExpressionXToken>(
                        _next.Location,
                        ["whitespace", "punctuation"]
                    );
                }
            }
            else
            {
                var beginIdentifier = _next.Location;
                while (_next.HasValue && char.IsLetterOrDigit(_next.Value))
                {
                    _next = _next.Remainder.ConsumeChar();
                }

                token = Result.Value(
                    new SExpressionXToken(SExpressionType.Atom),
                    beginIdentifier,
                    _next.Location
                );
            }

            _next = SkipWhiteSpace(_next.Location);
            return true;
        }

        public void Dispose()
        {
            // No resources to dispose of
        }
    }
}
