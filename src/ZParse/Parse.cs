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
using ZParse.Display;
using ZParse.Model;
using ZParse.Util;

namespace ZParse;

/// <summary>
/// General parsing helper methods.
/// </summary>
public static class Parse
{
    /// <summary>
    /// Parse a sequence of similar operands connected by left-associative operators.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
    /// <typeparam name="TOperator">The type of the operator.</typeparam>
    /// <param name="operator">A parser matching operators.</param>
    /// <param name="operand">A parser matching operands.</param>
    /// <param name="apply">A function combining an operator and two operands into the result.</param>
    /// <returns>The result of calling <paramref name="apply"/> successively on pairs of operands.</returns>
    /// <seealso cref="Combinators.Chain{TResult,TOperator,TOperand}"/>
    public static TextParser<T> Chain<T, TOperator>(
        TextParser<TOperator> @operator,
        TextParser<T> operand,
        Func<TOperator, T, T, T> apply
    )
    {
        return operand.Chain(@operator, operand, apply);
    }

    /// <summary>
    /// Parse a sequence of operands connected by right-associative operators.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
    /// <typeparam name="TOperator">The type of the operator.</typeparam>
    /// <param name="operator">A parser matching operators.</param>
    /// <param name="operand">A parser matching operands.</param>
    /// <param name="apply">A function combining an operator and two operands into the result.</param>
    /// <returns>The result of calling <paramref name="apply"/> successively on pairs of operands.</returns>
    public static TextParser<T> ChainRight<T, TOperator>(
        TextParser<TOperator> @operator,
        TextParser<T> operand,
        Func<TOperator, T, T, T> apply
    )
    {
        ArgumentNullException.ThrowIfNull(@operator);
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(apply);
        return operand.Then(first => ChainRightOperatorRest(first, @operator, operand, apply));
    }

    private static TextParser<T> ChainRightOperatorRest<T, TOperator>(
        T lastOperand,
        TextParser<TOperator> @operator,
        TextParser<T> operand,
        Func<TOperator, T, T, T> apply
    )
    {
        ArgumentNullException.ThrowIfNull(@operator);
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(apply);
        return @operator
            .Then(opvalue =>
                operand
                    .Then(operandValue =>
                        ChainRightOperatorRest(operandValue, @operator, operand, apply)
                    )
                    .Then(r => Return(apply(opvalue, lastOperand, r)))
            )
            .Or(Return(lastOperand));
    }

    /// <summary>
    /// Parse a sequence of similar operands connected by left-associative operators.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
    /// <typeparam name="TOperator">The type of the operator.</typeparam>
    /// <typeparam name="TKind">The kind of token being parsed.</typeparam>
    /// <param name="operator">A parser matching operators.</param>
    /// <param name="operand">A parser matching operands.</param>
    /// <param name="apply">A function combining an operator and two operands into the result.</param>
    /// <returns>The result of calling <paramref name="apply"/> successively on pairs of operands.</returns>
    /// <seealso cref="Combinators.Chain{TKind, TResult,TOperator,TOperand}"/>
    public static TokenListParser<TKind, T> Chain<TKind, T, TOperator>(
        TokenListParser<TKind, TOperator> @operator,
        TokenListParser<TKind, T> operand,
        Func<TOperator, T, T, T> apply
    )
    {
        return operand.Chain(@operator, operand, apply);
    }

    /// <summary>
    /// Parse a sequence of operands connected by right-associative operators.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
    /// <typeparam name="TOperator">The type of the operator.</typeparam>
    /// <typeparam name="TKind">The kind of token being parsed.</typeparam>
    /// <param name="operator">A parser matching operators.</param>
    /// <param name="operand">A parser matching operands.</param>
    /// <param name="apply">A function combining an operator and two operands into the result.</param>
    /// <returns>The result of calling <paramref name="apply"/> successively on pairs of operands.</returns>
    public static TokenListParser<TKind, T> ChainRight<TKind, T, TOperator>(
        TokenListParser<TKind, TOperator> @operator,
        TokenListParser<TKind, T> operand,
        Func<TOperator, T, T, T> apply
    )
    {
        ArgumentNullException.ThrowIfNull(@operator);
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(apply);
        return operand.Then(first => ChainRightOperatorRest(first, @operator, operand, apply));
    }

    private static TokenListParser<TKind, T> ChainRightOperatorRest<TKind, T, TOperator>(
        T lastOperand,
        TokenListParser<TKind, TOperator> @operator,
        TokenListParser<TKind, T> operand,
        Func<TOperator, T, T, T> apply
    )
    {
        ArgumentNullException.ThrowIfNull(@operator);
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(apply);
        return @operator
            .Then(opvalue =>
                operand
                    .Then(operandValue =>
                        ChainRightOperatorRest(operandValue, @operator, operand, apply)
                    )
                    .Then(r => Return<TKind, T>(apply(opvalue, lastOperand, r)))
            )
            .Or(Return<TKind, T>(lastOperand));
    }

    /// <summary>
    /// Constructs a parser that will fail if the given parser succeeds,
    /// and will succeed if the given parser fails. In any case, it won't
    /// consume any input. It's like a negative look-ahead in a regular expression.
    /// </summary>
    /// <typeparam name="T">The result type of the given parser</typeparam>
    /// <param name="parser">The parser to wrap</param>
    /// <returns>A parser that is the negation of the given parser.</returns>
    public static TextParser<Unit> Not<T>(TextParser<T> parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        return input =>
        {
            var result = parser(input);

            return !result.HasValue
                ? Result.Value(Unit.Value, input, input)
                : Result.Empty<Unit>(
                    input,
                    $"unexpected successful parsing of `{input.Until(result.Remainder)}`"
                );
        };
    }

    /// <summary>
    /// Constructs a parser that will fail if the given parser succeeds,
    /// and will succeed if the given parser fails. In any case, it won't
    /// consume any input. It's like a negative look-ahead in a regular expression.
    /// </summary>
    /// <typeparam name="T">The result type of the given parser.</typeparam>
    /// <typeparam name="TKind">The kind of token being parsed.</typeparam>
    /// <param name="parser">The parser to wrap</param>
    /// <returns>A parser that is the negation of the given parser.</returns>
    public static TokenListParser<TKind, Unit> Not<TKind, T>(TokenListParser<TKind, T> parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        return input =>
        {
            var result = parser(input);

            if (!result.HasValue)
                return TokenListParserResult.Value(Unit.Value, input, input);
            // This is usually a success case for Not(), so the allocations here are a bit of a pity.

            var current = input.ConsumeToken();
            var last = result.Remainder.ConsumeToken();
            if (!current.HasValue)
                return TokenListParserResult.Empty<TKind, Unit>(
                    input,
                    "unexpected successful parsing"
                );
            var span = last.HasValue
                ? current.Value.Span.Source!.Substring(
                    current.Value.Position.Absolute,
                    last.Value.Position.Absolute - current.Value.Position.Absolute
                )
                : current.Value.Span.Source![current.Value.Position.Absolute..];

            return TokenListParserResult.Empty<TKind, Unit>(
                input,
                $"unexpected successful parsing of {Presentation.FormatLiteral(Friendly.Clip(span, 12))}"
            );
        };
    }

    /// <summary>
    /// Lazily construct a parser, so that circular dependencies are possible.
    /// </summary>
    /// <param name="reference">A function creating the parser, when required.</param>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <returns>A parser that lazily evaluates <paramref name="reference"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="reference"/> is null.</exception>
    public static TextParser<T> Ref<T>(Func<TextParser<T>> reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        TextParser<T>? parser = null;

        return i =>
        {
            parser ??= reference();

            return parser(i);
        };
    }

    /// <summary>
    /// Lazily construct a parser, so that circular dependencies are possible.
    /// </summary>
    /// <param name="reference">A function creating the parser, when required.</param>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <typeparam name="TKind">The kind of token being parsed.</typeparam>
    /// <returns>A parser that lazily evaluates <paramref name="reference"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="reference"/> is null.</exception>
    public static TokenListParser<TKind, T> Ref<TKind, T>(Func<TokenListParser<TKind, T>> reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        TokenListParser<TKind, T>? parser = null;

        return i =>
        {
            parser ??= reference();

            return parser(i);
        };
    }

    /// <summary>
    /// Construct a parser with a fixed value.
    /// </summary>
    /// <param name="value">The value returned by the parser.</param>
    /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
    /// <returns>The parser.</returns>
    public static TextParser<T> Return<T>(T value)
    {
        return input => Result.Value(value, input, input);
    }

    /// <summary>
    /// Construct a parser with a fixed value.
    /// </summary>
    /// <param name="value">The value returned by the parser.</param>
    /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
    /// <typeparam name="TKind">The kind of token being parsed.</typeparam>
    /// <returns>The parser.</returns>
    public static TokenListParser<TKind, T> Return<TKind, T>(T value)
    {
        return input => TokenListParserResult.Value(value, input, input);
    }

    /// <summary>
    /// Construct a parser applies two parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, (T1, T2)> Sequence<TKind, T1, T2>(
        TokenListParser<TKind, T1> parser1,
        TokenListParser<TKind, T2> parser2
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T1, (T1, T2)>(rt);

            var ru = parser2(rt.Remainder);
            return ru.HasValue
                ? TokenListParserResult.Value((rt.Value, ru.Value), input, ru.Remainder)
                : TokenListParserResult.CastEmpty<TKind, T2, (T1, T2)>(ru);
        };
    }

    /// <summary>
    /// Construct a parser applies three parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <typeparam name="T3">The type of the third value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <param name="parser3">The third parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, (T1, T2, T3)> Sequence<TKind, T1, T2, T3>(
        TokenListParser<TKind, T1> parser1,
        TokenListParser<TKind, T2> parser2,
        TokenListParser<TKind, T3> parser3
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);
        ArgumentNullException.ThrowIfNull(parser3);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T1, (T1, T2, T3)>(rt);

            var ru = parser2(rt.Remainder);
            if (!ru.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T2, (T1, T2, T3)>(ru);

            var rv = parser3(ru.Remainder);
            return rv.HasValue
                ? TokenListParserResult.Value((rt.Value, ru.Value, rv.Value), input, rv.Remainder)
                : TokenListParserResult.CastEmpty<TKind, T3, (T1, T2, T3)>(rv);
        };
    }

    /// <summary>
    /// Construct a parser applies four parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <typeparam name="T3">The type of the third value parsed.</typeparam>
    /// <typeparam name="T4">The type of the fourth value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <param name="parser3">The third parser to apply.</param>
    /// <param name="parser4">The fourth parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, (T1, T2, T3, T4)> Sequence<TKind, T1, T2, T3, T4>(
        TokenListParser<TKind, T1> parser1,
        TokenListParser<TKind, T2> parser2,
        TokenListParser<TKind, T3> parser3,
        TokenListParser<TKind, T4> parser4
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);
        ArgumentNullException.ThrowIfNull(parser3);
        ArgumentNullException.ThrowIfNull(parser4);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T1, (T1, T2, T3, T4)>(rt);

            var ru = parser2(rt.Remainder);
            if (!ru.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T2, (T1, T2, T3, T4)>(ru);

            var rv = parser3(ru.Remainder);
            if (!rv.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T3, (T1, T2, T3, T4)>(rv);

            var rw = parser4(rv.Remainder);
            return rw.HasValue
                ? TokenListParserResult.Value(
                    (rt.Value, ru.Value, rv.Value, rw.Value),
                    input,
                    rw.Remainder
                )
                : TokenListParserResult.CastEmpty<TKind, T4, (T1, T2, T3, T4)>(rw);
        };
    }

    /// <summary>
    /// Construct a parser applies five parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <typeparam name="T3">The type of the third value parsed.</typeparam>
    /// <typeparam name="T4">The type of the fourth value parsed.</typeparam>
    /// <typeparam name="T5">The type of the fifth value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <param name="parser3">The third parser to apply.</param>
    /// <param name="parser4">The fourth parser to apply.</param>
    /// <param name="parser5">The fifth parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, (T1, T2, T3, T4, T5)> Sequence<TKind, T1, T2, T3, T4, T5>(
        TokenListParser<TKind, T1> parser1,
        TokenListParser<TKind, T2> parser2,
        TokenListParser<TKind, T3> parser3,
        TokenListParser<TKind, T4> parser4,
        TokenListParser<TKind, T5> parser5
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);
        ArgumentNullException.ThrowIfNull(parser3);
        ArgumentNullException.ThrowIfNull(parser4);
        ArgumentNullException.ThrowIfNull(parser5);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T1, (T1, T2, T3, T4, T5)>(rt);

            var ru = parser2(rt.Remainder);
            if (!ru.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T2, (T1, T2, T3, T4, T5)>(ru);

            var rv = parser3(ru.Remainder);
            if (!rv.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T3, (T1, T2, T3, T4, T5)>(rv);

            var rw = parser4(rv.Remainder);
            if (!rw.HasValue)
                return TokenListParserResult.CastEmpty<TKind, T4, (T1, T2, T3, T4, T5)>(rw);

            var rx = parser5(rw.Remainder);
            return rx.HasValue
                ? TokenListParserResult.Value(
                    (rt.Value, ru.Value, rv.Value, rw.Value, rx.Value),
                    input,
                    rx.Remainder
                )
                : TokenListParserResult.CastEmpty<TKind, T5, (T1, T2, T3, T4, T5)>(rx);
        };
    }

    /// <summary>
    /// Construct a parser applies two parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<(T1, T2)> Sequence<T1, T2>(
        TextParser<T1> parser1,
        TextParser<T2> parser2
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return Result.CastEmpty<T1, (T1, T2)>(rt);

            var ru = parser2(rt.Remainder);
            return ru.HasValue
                ? Result.Value((rt.Value, ru.Value), input, ru.Remainder)
                : Result.CastEmpty<T2, (T1, T2)>(ru);
        };
    }

    /// <summary>
    /// Construct a parser applies three parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <typeparam name="T3">The type of the third value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <param name="parser3">The third parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<(T1, T2, T3)> Sequence<T1, T2, T3>(
        TextParser<T1> parser1,
        TextParser<T2> parser2,
        TextParser<T3> parser3
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);
        ArgumentNullException.ThrowIfNull(parser3);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return Result.CastEmpty<T1, (T1, T2, T3)>(rt);

            var ru = parser2(rt.Remainder);
            if (!ru.HasValue)
                return Result.CastEmpty<T2, (T1, T2, T3)>(ru);

            var rv = parser3(ru.Remainder);
            return rv.HasValue
                ? Result.Value((rt.Value, ru.Value, rv.Value), input, rv.Remainder)
                : Result.CastEmpty<T3, (T1, T2, T3)>(rv);
        };
    }

    /// <summary>
    /// Construct a parser applies four parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <typeparam name="T3">The type of the third value parsed.</typeparam>
    /// <typeparam name="T4">The type of the fourth value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <param name="parser3">The third parser to apply.</param>
    /// <param name="parser4">The fourth parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<(T1, T2, T3, T4)> Sequence<T1, T2, T3, T4>(
        TextParser<T1> parser1,
        TextParser<T2> parser2,
        TextParser<T3> parser3,
        TextParser<T4> parser4
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);
        ArgumentNullException.ThrowIfNull(parser3);
        ArgumentNullException.ThrowIfNull(parser4);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return Result.CastEmpty<T1, (T1, T2, T3, T4)>(rt);

            var ru = parser2(rt.Remainder);
            if (!ru.HasValue)
                return Result.CastEmpty<T2, (T1, T2, T3, T4)>(ru);

            var rv = parser3(ru.Remainder);
            if (!rv.HasValue)
                return Result.CastEmpty<T3, (T1, T2, T3, T4)>(rv);

            var rw = parser4(rv.Remainder);
            return rw.HasValue
                ? Result.Value((rt.Value, ru.Value, rv.Value, rw.Value), input, rw.Remainder)
                : Result.CastEmpty<T4, (T1, T2, T3, T4)>(rw);
        };
    }

    /// <summary>
    /// Construct a parser applies five parsers in order and returns a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The type of the first value parsed.</typeparam>
    /// <typeparam name="T2">The type of the second value parsed.</typeparam>
    /// <typeparam name="T3">The type of the third value parsed.</typeparam>
    /// <typeparam name="T4">The type of the fourth value parsed.</typeparam>
    /// <typeparam name="T5">The type of the fifth value parsed.</typeparam>
    /// <param name="parser1">The first parser to apply.</param>
    /// <param name="parser2">The second parser to apply.</param>
    /// <param name="parser3">The third parser to apply.</param>
    /// <param name="parser4">The fourth parser to apply.</param>
    /// <param name="parser5">The fifth parser to apply.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<(T1, T2, T3, T4, T5)> Sequence<T1, T2, T3, T4, T5>(
        TextParser<T1> parser1,
        TextParser<T2> parser2,
        TextParser<T3> parser3,
        TextParser<T4> parser4,
        TextParser<T5> parser5
    )
    {
        ArgumentNullException.ThrowIfNull(parser1);
        ArgumentNullException.ThrowIfNull(parser2);
        ArgumentNullException.ThrowIfNull(parser3);
        ArgumentNullException.ThrowIfNull(parser4);
        ArgumentNullException.ThrowIfNull(parser5);

        return input =>
        {
            var rt = parser1(input);
            if (!rt.HasValue)
                return Result.CastEmpty<T1, (T1, T2, T3, T4, T5)>(rt);

            var ru = parser2(rt.Remainder);
            if (!ru.HasValue)
                return Result.CastEmpty<T2, (T1, T2, T3, T4, T5)>(ru);

            var rv = parser3(ru.Remainder);
            if (!rv.HasValue)
                return Result.CastEmpty<T3, (T1, T2, T3, T4, T5)>(rv);

            var rw = parser4(rv.Remainder);
            if (!rw.HasValue)
                return Result.CastEmpty<T4, (T1, T2, T3, T4, T5)>(rw);

            var rx = parser5(rw.Remainder);
            return rx.HasValue
                ? Result.Value(
                    (rt.Value, ru.Value, rv.Value, rw.Value, rx.Value),
                    input,
                    rx.Remainder
                )
                : Result.CastEmpty<T5, (T1, T2, T3, T4, T5)>(rx);
        };
    }

    /// <summary>
    /// Creates a parser which applies one of the specified parsers.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parsers">The parsers to try from left to right.</param>
    /// <returns>A parser which applies one of the specified parsers.</returns>
    public static TokenListParser<TKind, T> OneOf<TKind, T>(
        params ReadOnlySpan<TokenListParser<TKind, T>> parsers
    )
    {
        if (parsers.Length == 0)
        {
            return i => TokenListParserResult.Empty<TKind, T>(TokenList<TKind>.Empty);
        }

        var c = parsers[0];
        for (var i = 1; i < parsers.Length; i++)
        {
            c = c.Or(parsers[i]);
        }

        return c;
    }

    /// <summary>
    /// Creates a parser which applies one of the specified parsers.
    /// </summary>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parsers">The parser to try from left to right.</param>
    /// <returns>A parser which applies one of the specified parsers.</returns>
    public static TextParser<T> OneOf<T>(params ReadOnlySpan<TextParser<T>> parsers)
    {
        if (parsers.Length == 0)
        {
            return i => Result.Empty<T>(TextSpan.None);
        }

        var c = parsers[0];
        for (var i = 1; i < parsers.Length; i++)
        {
            c = c.Or(parsers[i]);
        }

        return c;
    }
}
