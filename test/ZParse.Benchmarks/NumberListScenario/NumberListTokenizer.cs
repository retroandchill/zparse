using ZParse.Model;

namespace ZParse.Benchmarks.NumberListScenario;

public class NumberListTokenizer : Tokenizer<NumberListToken, NumberListTokenizer.Enumerator>
{
    public static NumberListTokenizer Instance { get; } = new();

    protected override Enumerator Tokenize(TextSpan span)
    {
        var next = SkipWhiteSpace(span);
        return new Enumerator(next);
    }

    public ref struct Enumerator : ITokenEnumerator<NumberListToken>
    {
        private Result<char> _next;

        internal Enumerator(Result<char> next)
        {
            _next = next;
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
                token = Result.Empty<NumberListToken>(_next.Location, ["digit"]);
            }

            _next = SkipWhiteSpace(_next.Location);
            return true;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
