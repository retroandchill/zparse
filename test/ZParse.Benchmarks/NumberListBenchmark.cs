using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Sprache;
using Xunit;
using ZParse.Benchmarks.NumberListScenario;
using ZParse.Model;
using ZParse.Parsers;

namespace ZParse.Benchmarks;

[MemoryDiagnoser]
public class NumberListBenchmark
{
    private const int NumbersLength = 1000;
    private static readonly string Numbers = string.Join(" ", Enumerable.Range(0, NumbersLength));
    private static readonly Input SpracheInput = new(Numbers);
    private static readonly TextSpan SuperpowerTextSpan = new(Numbers);

    private static void AssertComplete(int[] numbers)
    {
        Assert.Equal(NumbersLength, numbers.Length);
        for (var i = 0; i < NumbersLength; ++i)
            Assert.Equal(i, numbers[i]);
    }

    [Fact]
    public void Verify()
    {
        AssertComplete(StringSplitAndInt32Parse());
        AssertComplete(SpracheText().Value);
        AssertComplete(SuperpowerText().Value);
        AssertComplete(SuperpowerToken().Value);
    }

    [Fact]
    public void Benchmark()
    {
        BenchmarkRunner.Run<NumberListBenchmark>();
    }

    [Benchmark(Baseline = true)]
    public static int[] StringSplitAndInt32Parse()
    {
        var tokens = Numbers.Split(' ');
        var numbers = new int[tokens.Length];
        for (var i = 0; i < tokens.Length; ++i)
        {
            numbers[i] = int.Parse(tokens[i]);
        }

        return numbers;
    }

    private static readonly Parser<int[]> SpracheParser = Sprache
        .Parse.Number.Token()
        .Select(int.Parse)
        .Many()
        .Select(n => n.ToArray());

    [Benchmark]
    public static IResult<int[]> SpracheText()
    {
        return SpracheParser(SpracheInput);
    }

    private static readonly TextParser<int[]> SuperpowerTextParser = Span
        .WhiteSpace.Optional()
        .IgnoreThen(Numerics.IntegerInt32)
        .Many()
        .AtEnd();

    [Benchmark]
    public static Result<int[]> SuperpowerText()
    {
        return SuperpowerTextParser(SuperpowerTextSpan);
    }

    private static readonly TokenListParser<NumberListToken, int[]> SuperpowerTokenListParser =
        Token
            .EqualTo(NumberListToken.Number)
            .Apply(Numerics.IntegerInt32) // Slower that int.Parse(), but worth benchmarking
            .Many()
            .AtEnd();

    [Benchmark]
    public static TokenListParserResult<NumberListToken, int[]> SuperpowerToken()
    {
        return SuperpowerTokenListParser(NumberListTokenizer.Instance.Tokenize(Numbers));
    }
}
