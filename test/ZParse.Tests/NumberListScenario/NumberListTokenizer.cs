using System;
using System.Collections.Generic;
using ZParse.Model;
using ZParse.Parsers;

namespace ZParse.Tests.NumberListScenario;

internal class NumberListTokenizer(bool useCustomErrors = false) : Tokenizer<NumberListToken>
{
    protected override IEnumerable<Result<NumberListToken>> Tokenize(TextSpan span)
    {
        var next = SkipWhiteSpace(span);
        if (!next.HasValue)
            yield break;

        do
        {
            var ch = next.Value;
            if (ch is >= '0' and <= '9')
            {
                var integer = Numerics.Integer(next.Location);
                next = integer.Remainder.ConsumeChar();
                yield return Result.Value(
                    NumberListToken.Number,
                    integer.Location,
                    integer.Remainder
                );
            }
            else
            {
                if (useCustomErrors)
                {
                    yield return Result.Empty<NumberListToken>(
                        next.Location,
                        "list must contain only numbers"
                    );
                }
                else
                {
                    yield return Result.Empty<NumberListToken>(next.Location, ["digit"]);
                }
            }

            next = SkipWhiteSpace(next.Location);
        } while (next.HasValue);
    }
}
