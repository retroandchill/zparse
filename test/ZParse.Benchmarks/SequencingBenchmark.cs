using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Xunit;
using ZParse.Model;
using ZParse.Parsers;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace ZParse.Benchmarks;

[MemoryDiagnoser]
public class SequencingBenchmark
{
    private const string Numbers = "123";
    private static TextSpan Input => new(Numbers);

    private static void AssertValues((char First, char Second, char Third) numbers)
    {
        Assert.Equal('1', numbers.First);
        Assert.Equal('2', numbers.Second);
        Assert.Equal('3', numbers.Third);
    }

    [Fact]
    public void Verify()
    {
        AssertValues(ApplyThen().Value);

        var (c1, c2, c3) = ApplySequence().Value;
        AssertValues((c1, c2, c3));
    }

    [Fact]
    public void Benchmark()
    {
        BenchmarkRunner.Run<SequencingBenchmark>();
    }

    private static readonly TextParser<(char, char, char)> ThenParser = Character.Digit.Then(
        first =>
            Character.Digit.Then(second =>
                Character.Digit.Then(third => Parse.Return((first, second, third)))
            )
    );

    [Benchmark(Baseline = true)]
    public static Result<(char, char, char)> ApplyThen()
    {
        return ThenParser(Input);
    }

    private static readonly TextParser<RefTuple<char, char, char>> SequenceParser = Parse
        .Sequence(Character.Digit, Character.Digit, Character.Digit)
        .Select(t => t); // Even up the work done

    [Benchmark]
    public static Result<RefTuple<char, char, char>> ApplySequence()
    {
        return SequenceParser(Input);
    }
}
