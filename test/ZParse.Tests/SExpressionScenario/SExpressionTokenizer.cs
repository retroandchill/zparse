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
        private bool _consumingCharacterSuffix;

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

            if (_consumingCharacterSuffix)
            {
                _consumingCharacterSuffix = false;

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

                    _next = SkipWhiteSpace(_next.Location);
                    return true;
                }

                _next = SkipWhiteSpace(_next.Location);
            }

            switch (_next.Value)
            {
                case '(':
                    token = Result.Value(SExpressionToken.LParen, _next.Location, _next.Remainder);
                    _next = _next.Remainder.ConsumeChar();
                    break;
                case ')':
                    token = Result.Value(SExpressionToken.RParen, _next.Location, _next.Remainder);
                    _next = _next.Remainder.ConsumeChar();
                    break;
                case >= '0' and <= '9':
                {
                    var integer = Numerics.Integer(_next.Location);
                    _next = integer.Remainder.ConsumeChar();

                    token = Result.Value(
                        SExpressionToken.Number,
                        integer.Location,
                        integer.Remainder
                    );
                    _consumingCharacterSuffix = true;
                    return true;
                }
                default:
                {
                    var beginIdentifier = _next.Location;
                    while (_next.HasValue && char.IsLetterOrDigit(_next.Value))
                    {
                        _next = _next.Remainder.ConsumeChar();
                    }

                    token = Result.Value(SExpressionToken.Atom, beginIdentifier, _next.Location);
                    break;
                }
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
