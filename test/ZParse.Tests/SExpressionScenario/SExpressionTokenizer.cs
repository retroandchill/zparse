using System.Collections.Generic;
using ZParse.Model;
using ZParse.Parsers;

namespace ZParse.Tests.SExpressionScenario;

internal class SExpressionTokenizer : Tokenizer<SExpressionToken, SExpressionTokenizer.Enumerator>
{
    protected override Enumerator Tokenize(TextSpan span)
    {
        var next = SkipWhiteSpace(span);
        return new Enumerator(next);
    }

    public ref struct Enumerator : ITokenEnumerator<SExpressionToken>
    {
        private Result<char> _next;

        internal Enumerator(Result<char> next)
        {
            _next = next;
        }

        public bool NextToken(out Result<SExpressionToken> token)
        {
            if (!_next.HasValue)
            {
                token = default;
                return false;
            }

            if (_next.Value == '(')
            {
                token = Result.Value(SExpressionToken.LParen, _next.Location, _next.Remainder);
                _next = _next.Remainder.ConsumeChar();
            }
            else if (_next.Value == ')')
            {
                token = Result.Value(SExpressionToken.RParen, _next.Location, _next.Remainder);
                _next = _next.Remainder.ConsumeChar();
            }
            else if (_next.Value is >= '0' and <= '9')
            {
                var integer = Numerics.Integer(_next.Location);
                _next = integer.Remainder.ConsumeChar();

                token = Result.Value(SExpressionToken.Number, integer.Location, integer.Remainder);

                if (
                    _next.HasValue
                    && !char.IsPunctuation(_next.Value)
                    && !char.IsWhiteSpace(_next.Value)
                )
                {
                    token = Result.Empty<SExpressionToken>(
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

                token = Result.Value(SExpressionToken.Atom, beginIdentifier, _next.Location);
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
