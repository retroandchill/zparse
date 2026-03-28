using ZParse.Model;

namespace ZParse.Tests.NumberListScenario;

internal class NumberListTokenizer(bool useCustomErrors = false)
    : Tokenizer<NumberListToken, NumberListTokenizer.Enumerator>
{
    public static NumberListTokenizer Instance { get; } = new();

    protected override Enumerator Tokenize(TextSpan span)
    {
        var next = SkipWhiteSpace(span);
        return new Enumerator(next, useCustomErrors);
    }

    public ref struct Enumerator : ITokenEnumerator<NumberListToken>
    {
        private Result<char> _next;
        private readonly bool _useCustomErrors;

        internal Enumerator(Result<char> next, bool useCustomErrors = false)
        {
            _next = next;
            _useCustomErrors = useCustomErrors;
        }

        public bool NextToken(out Result<NumberListToken> token)
        {
            if (!_next.HasValue)
            {
                token = default;
                return false;
            }

            var ch = _next.Value;
            if (ch is >= '0' and <= '9')
            {
                var start = _next;
                _next = _next.Remainder.ConsumeChar();
                while (_next.HasValue && _next.Value >= '0' && _next.Value <= '9')
                {
                    _next = _next.Remainder.ConsumeChar();
                }
                token = Result.Value(NumberListToken.Number, start.Location, _next.Location);
            }
            else
            {
                if (_useCustomErrors)
                {
                    token = Result.Empty<NumberListToken>(
                        _next.Location,
                        "list must contain only numbers"
                    );
                }
                else
                {
                    token = Result.Empty<NumberListToken>(_next.Location, ["digit"]);
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
