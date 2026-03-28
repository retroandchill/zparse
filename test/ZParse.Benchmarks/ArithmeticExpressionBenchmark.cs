using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Sprache;
using Xunit;
using ZParse.Benchmarks.ArithmeticExpressionScenario;
using ZParse.Model;

namespace ZParse.Benchmarks;

[MemoryDiagnoser]
public class ArithmeticExpressionBenchmark
{
    // This benchmark includes construction of the input, and unwrapping of results, while
    // NumberListBenchmark does not.

    private static readonly ArithmeticExpressionTokenizer Tokenizer = new();
    private const string Expression =
        "123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456";
    private const int ExpectedValue = 280095;

    [Fact]
    public void Verify()
    {
        Assert.Equal(ExpectedValue, SpracheText().Compile()());
        Assert.Equal(ExpectedValue, SuperpowerTokenListParser().Compile()());
        Assert.Equal(ExpectedValue, SuperpowerComplete().Compile()());
    }

    [Fact]
    public void Benchmark()
    {
        BenchmarkRunner.Run<ArithmeticExpressionBenchmark>();
    }

    [Benchmark(Baseline = true)]
    public static Expression<Func<int>> SpracheText()
    {
        return SpracheArithmeticExpressionParser.Lambda.Parse(Expression);
    }

    [Benchmark]
    public static TokenList<ArithmeticExpressionToken> SuperpowerTokenizer()
    {
        return Tokenizer.Tokenize(Expression);
    }

    [Benchmark]
    public static Expression<Func<int>> SuperpowerTokenListParser()
    {
        var tokens = Tokenizer.Tokenize(Expression);
        return ArithmeticExpressionParser.Lambda.Parse(tokens);
    }

    [Benchmark]
    public static Expression<Func<int>> SuperpowerComplete()
    {
        return ArithmeticExpressionParser.Lambda.Parse(Tokenizer.Tokenize(Expression));
    }
}
