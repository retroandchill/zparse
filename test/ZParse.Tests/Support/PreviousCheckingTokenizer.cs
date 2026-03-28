using System.Collections.Generic;
using Xunit;
using ZParse.Model;

namespace ZParse.Tests.Support;

public class PreviousCheckingTokenizer : Tokenizer<int, PreviousCheckingTokenizer.Enumerator>
{
    protected override Enumerator Tokenize(TextSpan span, TokenizationState<int> state)
    {
        return new Enumerator(span, state);
    }

    public ref struct Enumerator(TextSpan span, TokenizationState<int> state)
        : ITokenEnumerator<int>
    {
        private readonly TextSpan _span = span;
        private TextSpan _remainder = span;
        private Result<char> _next = default;
        private int _index = 0;

        public bool NextToken(out Result<int> token)
        {
            if (_index >= _span.Length)
            {
                token = default;
                return false;
            }

            if (_index == 0)
            {
                Assert.NotNull(state);
                Assert.Null(state.Previous);
                _next = _span.ConsumeChar();
                token = Result.Value(0, _next.Location, _next.Remainder);
                _index++;
                return true;
            }

            Assert.NotNull(state.Previous);
            Assert.Equal(_index - 1, state.Previous!.Value.Kind);
            _next = _next.Remainder.ConsumeChar();
            token = Result.Value(_index, _next.Location, _next.Remainder);
            _index++;
            return true;
        }

        public void Dispose()
        {
            // No resources to dispose of
        }
    }
}
