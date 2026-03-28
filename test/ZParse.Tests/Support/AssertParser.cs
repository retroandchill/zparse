// Copyright 2016 Datalust, Superpower Contributors, Sprache Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ZParse.Model;

namespace ZParse.Tests.Support;

internal static class AssertParser
{
    public static void SucceedsWithMany<T>(
        TextParser<T[]> parser,
        string input,
        IEnumerable<T> expectedResult
    )
    {
        Succeeds(parser, input, t => Assert.True(t.SequenceEqual(expectedResult)));
    }

    public static void SucceedsWithAll(TextParser<TextSpan> parser, string input)
    {
        SucceedsWithAll(parser.Select(s => s.ToStringValue().ToCharArray()), input);
    }

    public static void SucceedsWithAll(TextParser<char[]> parser, string input)
    {
        SucceedsWithMany(parser, input, input.ToCharArray());
    }

    private static void Succeeds<T>(TextParser<T> parser, string input, Action<T> resultAssertion)
    {
        var t = parser.Parse(input);
        resultAssertion(t);
    }

    public static void SucceedsWith<T>(TextParser<T> parser, string input, T value)
    {
        var t = parser.Parse(input);
        Assert.Equal(value, t);
    }

    public static void Fails<T>(TextParser<T> parser, string input)
        where T : allows ref struct
    {
        FailsWith(parser, input, _ => { });
    }

    public static void FailsAt<T>(TextParser<T> parser, string input, int position)
        where T : allows ref struct
    {
        FailsWith(parser, input, f => Assert.Equal(position, f.Remainder.Position.Absolute));
    }

    private static void FailsWith<T>(
        TextParser<T> parser,
        string input,
        Action<Result<T>> resultAssertion
    )
        where T : allows ref struct
    {
        var result = parser.TryParse(input);

        if (result.HasValue)
        {
            var asString = Result<T>.Stringify?.Invoke(result.Value);
            var userMessage = asString is not null
                ? $"Expected failure but succeeded with {asString}."
                : $"Expected failure but succeeded.";
            Assert.False(result.HasValue, userMessage);
        }

        resultAssertion(result);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    public static void FailsWithMessage<T>(TextParser<T> parser, string input, string message)
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        FailsWith(
            parser,
            input,
            r =>
            {
                Assert.Equal(message, r.ToString());
            }
        );
    }

    private static void SucceedsWithMany<T>(
        TokenListParser<char, T[]> parser,
        string input,
        IEnumerable<T> expectedResult
    )
    {
        Succeeds(parser, input, t => Assert.True(t.SequenceEqual(expectedResult)));
    }

    public static void SucceedsWithMany(
        TokenListParser<char, Token<char>[]> parser,
        string input,
        IEnumerable<char> expectedResult
    )
    {
        Succeeds(
            parser,
            input,
            t => Assert.True(t.Select(tok => tok.Kind).SequenceEqual(expectedResult))
        );
    }

    public static void SucceedsWithAll(TokenListParser<char, Token<char>[]> parser, string input)
    {
        SucceedsWithMany(
            parser.Select(t => t.Select(tk => tk.Kind).ToArray()),
            input,
            input.ToCharArray()
        );
    }

    private static void Succeeds<T>(
        TokenListParser<char, T> parser,
        string input,
        Action<T> resultAssertion
    )
    {
        var t = parser.Parse(StringAsCharTokenList.Tokenize(input));
        resultAssertion(t);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    public static void SucceedsWith(
        TokenListParser<char, Token<char>> parser,
        string input,
        char value
    )
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        Succeeds(
            parser,
            input,
            tok =>
            {
                Assert.Equal(value, tok.Kind);
            }
        );
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    public static void SucceedsWith<T>(TokenListParser<char, T> parser, string input, T value)
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        Succeeds(
            parser,
            input,
            v =>
            {
                Assert.Equal(value, v);
            }
        );
    }

    public static void Fails<T>(TokenListParser<char, T> parser, string input)
    {
        FailsWith(parser, input, _ => { });
    }

    public static void FailsAt<T>(TokenListParser<char, T> parser, string input, int position)
    {
        FailsWith(parser, input, f => Assert.Equal(position, f.Remainder.Position));
    }

    private static void FailsWith<T>(
        TokenListParser<char, T> parser,
        string input,
        Action<TokenListParserResult<char, T>> resultAssertion
    )
    {
        var result = parser.TryParse(StringAsCharTokenList.Tokenize((input)));

        if (result.HasValue)
            Assert.False(result.HasValue, $"Expected failure but succeeded with {result.Value}.");

        resultAssertion(result);
    }

    public static void FailsWithMessage<TKind, T>(
        TokenListParser<TKind, T> parser,
        string input,
        ITokenizer<TKind> tokenizer,
        string message
    )
        where T : allows ref struct
    {
        var result = parser.TryParse(tokenizer.Tokenize(input));
        Assert.Equal(message, result.ToString());
    }

    public static void FailsWithMessage<T>(
        TokenListParser<char, T> parser,
        string input,
        string message
    )
    {
        var result = parser.TryParse(StringAsCharTokenList.Tokenize(input));
        Assert.Equal(message, result.ToString());
    }

    public static void FitsTheory(TextParser<TextSpan> parser, string input, bool isMatch)
    {
        if (isMatch)
            SucceedsWithAll(parser, input);
        else
            Fails(parser.AtEnd(), input);
    }
}
