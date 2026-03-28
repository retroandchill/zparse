using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Xunit;
using ZLinq;
using ZParse.Benchmarks.NumberListScenario;
using ZParse.Model;
using ZParse.Parsers;
using ZParse.Tokenizers;

namespace ZParse.Benchmarks;

[MemoryDiagnoser]
public class TokenizerBuilderBenchmark
{
    private const int NumbersLength = 1000;
    private static readonly string Numbers = string.Join(" ", Enumerable.Range(0, NumbersLength));

    private static readonly ITokenizer<NumberListToken> BuilderTokenizer =
        new TokenizerBuilder<NumberListToken>()
            .Match(Numerics.Integer, NumberListToken.Number)
            .Ignore(Span.WhiteSpace)
            .Build();

    private static void AssertComplete(TokenList<NumberListToken> numbers)
    {
        var tokens = numbers.AsValueEnumerable().ToArray();
        Assert.Equal(NumbersLength, tokens.Length);
        for (var i = 0; i < NumbersLength; ++i)
        {
            Assert.Equal(NumberListToken.Number, tokens[i].Kind);
            Assert.Equal(i.ToString(), tokens[i].ToStringValue(numbers.Source));
        }
    }

    [Fact]
    public void Verify()
    {
        AssertComplete(HandCoded());
        AssertComplete(Builder());
    }

    [Fact]
    public void Benchmark()
    {
        BenchmarkRunner.Run<TokenizerBuilderBenchmark>();
    }

    [Benchmark(Baseline = true)]
    public static TokenList<NumberListToken> HandCoded()
    {
        return NumberListTokenizer.Instance.Tokenize(Numbers);
    }

    [Benchmark]
    public static TokenList<NumberListToken> Builder()
    {
        return BuilderTokenizer.Tokenize(Numbers);
    }
}
