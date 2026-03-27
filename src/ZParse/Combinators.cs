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
using ZParse.Display;
using ZParse.Model;
using ZParse.Util;

// ReSharper disable MemberCanBePrivate.Global

namespace ZParse;

/// <summary>
/// Functions that construct more complex parsers by combining simpler ones.
/// </summary>
public static class Combinators
{
    /// <param name="parser">The parser.</param>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    extension<TKind>(TokenListParser<TKind, Token<TKind>> parser)
    {
        /// <summary>
        /// Apply the text parser <paramref name="valueParser"/> to the span represented by the parsed token.
        /// </summary>
        /// <typeparam name="T">The type of the resulting value.</typeparam>
        /// <param name="valueParser">A function that determines which text parser to apply.</param>
        /// <returns>A parser that returns the result of parsing the token value.</returns>
        public TokenListParser<TKind, T> Apply<T>(Func<Token<TKind>, TextParser<T>> valueParser)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(valueParser);
            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return TokenListParserResult.CastEmpty<TKind, Token<TKind>, T>(rt);

                var uParser = valueParser(rt.Value);
                var uResult = uParser.AtEnd()(rt.Value.Span);
                if (uResult.HasValue)
                    return TokenListParserResult.Value(uResult.Value, rt.Location, rt.Remainder);

                var message =
                    $"invalid {Presentation.FormatExpectation(rt.Value.Kind)}, {uResult.FormatErrorMessageFragment()}";
                return new TokenListParserResult<TKind, T>(
                    input,
                    uResult.Remainder.Position,
                    message,
                    [],
                    uResult.Backtrack
                );
            };
        }

        /// <summary>
        /// Apply the text parser <paramref name="valueParser"/> to the span represented by the parsed token.
        /// </summary>
        /// <typeparam name="T">The type of the resulting value.</typeparam>
        /// <param name="valueParser">A text parser to apply.</param>
        /// <returns>A parser that returns the result of parsing the token value.</returns>
        public TokenListParser<TKind, T> Apply<T>(TextParser<T> valueParser)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(valueParser);

            var valueParserAtEnd = valueParser.AtEnd();
            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return TokenListParserResult.CastEmpty<TKind, Token<TKind>, T>(rt);

                var uResult = valueParserAtEnd(rt.Value.Span);
                if (uResult.HasValue)
                    return TokenListParserResult.Value(uResult.Value, rt.Location, rt.Remainder);

                var problem = uResult.Remainder.IsAtEnd ? "incomplete" : "invalid";
                var textError = uResult.Remainder.IsAtEnd
                    ? !uResult.Expectations.IsDefaultOrEmpty
                        ? $", expected {Friendly.List(uResult.Expectations)}"
                        : ""
                    : $", {uResult.FormatErrorMessageFragment()}";
                var message =
                    $"{problem} {Presentation.FormatExpectation(rt.Value.Kind)}{textError}";
                return new TokenListParserResult<TKind, T>(
                    input,
                    rt.Remainder,
                    uResult.Remainder.Position,
                    message,
                    [],
                    uResult.Backtrack
                );
            };
        }
    }

    /// <summary>
    /// Apply the text parser <paramref name="valueParser"/> to the span
    /// captured by the parser.
    /// </summary>
    /// <typeparam name="T">The type of the resulting value.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <param name="valueParser">A text parser to apply to the span.</param>
    /// <returns>A parser that returns the result of parsing the span value.</returns>
    public static TextParser<T> Apply<T>(
        this TextParser<TextSpan> parser,
        TextParser<T> valueParser
    )
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(valueParser);
        return input =>
        {
            var rt = parser(input);
            if (!rt.HasValue)
                return Result.CastEmpty<TextSpan, T>(rt);

            var uResult = valueParser(rt.Value);

            if (!uResult.HasValue)
            {
                var errloc = rt.Value.Source != input.Source ? rt.Location : uResult.Location;
                return new Result<T>(
                    errloc,
                    rt.Remainder,
                    uResult.ErrorMessage,
                    uResult.Expectations,
                    false
                );
            }

            if (uResult.Remainder.IsAtEnd)
                return Result.Value(uResult.Value, rt.Location, rt.Remainder);

            var loc = rt.Value.Source != input.Source ? rt.Location : uResult.Remainder;
            return Result.Empty<T>(loc);
        };
    }

    /// <param name="parser">The parser.</param>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    extension<TKind, T>(TokenListParser<TKind, T> parser)
    {
        /// <summary>
        /// Construct a parser that succeeds only if the source is at the end of input.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T> AtEnd()
        {
            ArgumentNullException.ThrowIfNull(parser);

            return input =>
            {
                var result = parser(input);
                if (!result.HasValue || result.Remainder.IsAtEnd)
                    return result;

                return TokenListParserResult.Empty<TKind, T>(result.Remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches one or more instances of applying <paramref name="parser"/>.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T[]> AtLeastOnce()
        {
            ArgumentNullException.ThrowIfNull(parser);
            return parser.Then(first =>
                parser.Many().Select(rest => ArrayEnumerable.Cons(first, rest))
            );
        }

        /// <summary>
        /// Construct a parser that matches a specified number of instances of applying <paramref name="parser"/>.
        /// </summary>
        /// <param name="count">The number of times to apply <paramref name="parser"/>.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T[]> Repeat(int count)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            return input =>
            {
                // Assuming we'll try the parser and fail quite often, allocating the result
                // array lazily should save some allocs for not much effort here.
                T[]? result = null;
                var remainder = input;
                for (var i = 0; i < count; ++i)
                {
                    var r = parser(remainder);
                    if (!r.HasValue)
                        return TokenListParserResult.CastEmpty<TKind, T, T[]>(r);

                    result ??= new T[count];
                    result[i] = r.Value;
                    remainder = r.Remainder;
                }

                return TokenListParserResult.Value(result ?? Array.Empty<T>(), input, remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches one or more instances of applying <paramref name="parser"/>, delimited by <paramref name="delimiter"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="delimiter">The parser that matches the delimiters.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T[]> AtLeastOnceDelimitedBy<TOther>(
            TokenListParser<TKind, TOther> delimiter
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(delimiter);

            return parser.Then(first =>
                delimiter
                    .IgnoreThen(parser)
                    .Many()
                    .Select(rest => ArrayEnumerable.Cons(first, rest))
            );
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="left"/>, discards the resulting value,
        /// then matches <paramref name="parser"/>, keeps the value, then matches <paramref name="right"/>
        /// and returns the value matched by <paramref name="parser"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="left">First parser to match, value is ignored.</param>
        /// <param name="right">Last parser to match, value is ignored.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T> Between<TOther>(
            TokenListParser<TKind, TOther> left,
            TokenListParser<TKind, TOther> right
        )
        {
            return left.IgnoreThen(parser.Then(right.Value));
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/>, discards the resulting value, then returns the result of <paramref name="second"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="second">The second parser.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, TOther> IgnoreThen<TOther>(
            TokenListParser<TKind, TOther> second
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(second);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return TokenListParserResult.CastEmpty<TKind, T, TOther>(rt);

                var ru = second(rt.Remainder);
                return !ru.HasValue
                    ? ru
                    : TokenListParserResult.Value(ru.Value, input, ru.Remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/> zero or more times, delimited by <paramref name="delimiter"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="delimiter">The parser that matches the delimiters.</param>
        /// <param name="end">A parser to match a final trailing delimiter, if required. Specifying
        /// this can improve error reporting for some lists.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T[]> ManyDelimitedBy<TOther>(
            TokenListParser<TKind, TOther> delimiter,
            TokenListParser<TKind, TOther>? end = null
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(delimiter);

            // ReSharper disable once ConvertClosureToMethodGroup

            if (end is not null)
                return parser
                    .AtLeastOnceDelimitedBy(delimiter)
                    .Then(p => end.Value(p))
                    .Or(end.Value(Array.Empty<T>()));

            return parser
                .Then(first =>
                    delimiter
                        .IgnoreThen(parser)
                        .Many()
                        .Select(rest => ArrayEnumerable.Cons(first, rest))
                )
                .OptionalOrDefault([]);
        }

        /// <summary>
        /// Construct a parser that fails with error message <paramref name="errorMessage"/> when <paramref name="parser"/> fails.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T> Message(string errorMessage)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(errorMessage);

            return input =>
            {
                var result = parser(input);
                return result.HasValue
                    ? result
                    : TokenListParserResult.Empty<TKind, T>(
                        result.Remainder,
                        result.SubTokenErrorPosition,
                        errorMessage
                    );
            };
        }

        /// <summary>
        /// Construct a parser that returns <paramref name="name"/> as its "expectation" if <paramref name="parser"/> fails.
        /// </summary>
        /// <param name="name">The name given to <paramref name="parser"/>.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T> Named(string name)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(name);

            return input =>
            {
                var result = parser(input);
                if (result.HasValue || result.IsPartial(input))
                    return result;

                return TokenListParserResult.Empty<TKind, T>(result.Remainder, [name]);
            };
        }

        /// <summary>
        /// Construct a parser that takes the result of <paramref name="parser"/> and converts it value using <paramref name="selector"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="selector">A mapping from the first result to the second.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, TOther> Select<TOther>(Func<T, TOther> selector)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(selector);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return TokenListParserResult.CastEmpty<TKind, T, TOther>(rt);

                var u = selector(rt.Value);

                return TokenListParserResult.Value(u, input, rt.Remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches zero or one instance of <paramref name="parser"/>, returning <paramref name="defaultValue"/> when
        /// no match is possible.
        /// </summary>
        /// <param name="defaultValue">The default value</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T> OptionalOrDefault(T defaultValue = default!)
        {
            ArgumentNullException.ThrowIfNull(parser);

            return parser.Or(Parse.Return<TKind, T>(defaultValue!));
        }

        /// <summary>
        /// Construct a parser that tries first the <paramref name="parser"/> parser, and if it fails, applies <paramref name="rhs"/>.
        /// </summary>
        /// <param name="rhs">The second parser to try.</param>
        /// <returns>The resulting parser.</returns>
        /// <remarks>Or will fail if the first item partially matches this. To modify this behavior use <see cref="Try{TKind,T}(TokenListParser{TKind,T})"/>.</remarks>
        public TokenListParser<TKind, T> Or(TokenListParser<TKind, T> rhs)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(rhs);

            return input =>
            {
                var first = parser(input);
                if (first.HasValue || !first.Backtrack && first.IsPartial(input))
                    return first;

                var second = rhs(input);
                return second.HasValue ? second : TokenListParserResult.CombineEmpty(first, second);
            };
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/> zero or more times.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        /// <remarks>Many will fail if any item partially matches this. To modify this behavior use <see cref="Try{TKind,T}(TokenListParser{TKind,T})"/>.</remarks>
        public TokenListParser<TKind, T[]> Many()
        {
            ArgumentNullException.ThrowIfNull(parser);

            return input =>
            {
                var result = new List<T>();
                var from = input;
                var r = parser(input);
                while (r.HasValue)
                {
                    if (from == r.Remainder) // Broken parser, not a failed parsing.
                        throw new ParseException(
                            $"Many() cannot be applied to zero-width parsers; value {r.Value} at position {r.Location.Position}.",
                            r.ErrorPosition
                        );

                    result.Add(r.Value);
                    from = r.Remainder;
                    r = parser(r.Remainder);
                }

                if (!r.Backtrack && r.IsPartial(from))
                    return TokenListParserResult.CastEmpty<TKind, T, T[]>(r);

                return TokenListParserResult.Value(result.ToArray(), input, from);
            };
        }

        /// <summary>
        /// The LINQ query comprehension pattern.
        /// </summary>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="selector">A mapping from the first result to the second parser.</param>
        /// <param name="projector">Function mapping the results of the first two parsers onto the final result.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, TOutput> SelectMany<TResult, TOutput>(
            Func<T, TokenListParser<TKind, TResult>> selector,
            Func<T, TResult, TOutput> projector
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(projector);

            return parser.Then(t => selector(t).Select(u => projector(t, u)));
        }

        /// <summary>
        /// Construct a parser that applies <paramref name="parser"/>, provides the value to <paramref name="second"/> and returns the result.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="second">The second parser.</param>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, TOther> Then<TOther>(
            Func<T, TokenListParser<TKind, TOther>> second
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(second);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return TokenListParserResult.CastEmpty<TKind, T, TOther>(rt);

                var ru = second(rt.Value)(rt.Remainder);
                return !ru.HasValue
                    ? ru
                    : TokenListParserResult.Value(ru.Value, input, ru.Remainder);
            };
        }

        /// <summary>
        /// Parse a sequence of operands connected by left-associative operators.
        /// </summary>
        /// <typeparam name="TOperator">The type of the operator.</typeparam>
        /// <typeparam name="TOperand">The type of subsequent operands.</typeparam>
        /// <param name="operator">A parser matching operators.</param>
        /// <param name="operand">A parser matching operands.</param>
        /// <param name="apply">A function combining the operator, left operand, and right operand, into the result.</param>
        /// <returns>The result of calling <paramref name="apply"/> successively on pairs of operands.</returns>
        public TokenListParser<TKind, T> Chain<TOperator, TOperand>(
            TokenListParser<TKind, TOperator> @operator,
            TokenListParser<TKind, TOperand> operand,
            Func<TOperator, T, TOperand, T> apply
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(@operator);
            ArgumentNullException.ThrowIfNull(operand);
            ArgumentNullException.ThrowIfNull(apply);

            return input =>
            {
                var parseResult = parser(input);
                if (!parseResult.HasValue)
                    return parseResult;

                var result = parseResult.Value;
                var operandRemainder = parseResult.Remainder;

                var operatorResult = @operator(operandRemainder);
                while (operatorResult.HasValue || operatorResult.IsPartial(operandRemainder))
                {
                    // If operator read any input, but failed to read complete input, we return error
                    if (!operatorResult.HasValue)
                        return TokenListParserResult.CastEmpty<TKind, TOperator, T>(operatorResult);

                    var operandResult = operand(operatorResult.Remainder);
                    operandRemainder = operandResult.Remainder;

                    if (!operandResult.HasValue)
                        return TokenListParserResult.CastEmpty<TKind, TOperand, T>(operandResult);

                    result = apply(operatorResult.Value, result, operandResult.Value);

                    operatorResult = @operator(operandRemainder);
                }

                return TokenListParserResult.Value(result, input, operandRemainder);
            };
        }

        /// <summary>
        /// Construct a parser that tries one parser, and backtracks if unsuccessful so that no input
        /// appears to have been consumed by subsequent checks against the result.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        public TokenListParser<TKind, T> Try()
        {
            ArgumentNullException.ThrowIfNull(parser);

            return input =>
            {
                var rt = parser(input);
                if (rt.HasValue)
                    return rt;

                rt.Backtrack = true;
                return rt;
            };
        }
    }

    /// <param name="parser">The parser.</param>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    extension<T>(TextParser<T> parser)
    {
        /// <summary>
        /// Construct a parser that succeeds only if the source is at the end of input.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        public TextParser<T> AtEnd()
        {
            ArgumentNullException.ThrowIfNull(parser);

            return input =>
            {
                var result = parser(input);
                if (!result.HasValue || result.Remainder.IsAtEnd)
                    return result;

                return Result.Empty<T>(result.Remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches one or more instances of applying <paramref name="parser"/>.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        public TextParser<T[]> AtLeastOnce()
        {
            ArgumentNullException.ThrowIfNull(parser);
            return parser.Then(first =>
                parser.Many().Select(rest => ArrayEnumerable.Cons(first, rest))
            );
        }

        /// <summary>
        /// Construct a parser that matches a specified number of instances of applying <paramref name="parser"/>.
        /// </summary>
        /// <param name="count">The number of times to apply <paramref name="parser"/>.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T[]> Repeat(int count)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            return input =>
            {
                // Assuming we'll try the parser and fail quite often, allocating the result
                // array lazily should save some allocs for not much effort here.
                T[]? result = null;
                var remainder = input;
                for (var i = 0; i < count; ++i)
                {
                    var r = parser(remainder);
                    if (!r.HasValue)
                        return Result.CastEmpty<T, T[]>(r);

                    result ??= new T[count];
                    result[i] = r.Value;
                    remainder = r.Remainder;
                }

                return Result.Value(result ?? [], input, remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches one or more instances of applying <paramref name="parser"/>, delimited by <paramref name="delimiter"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="delimiter">The parser that matches the delimiters.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T[]> AtLeastOnceDelimitedBy<TOther>(TextParser<TOther> delimiter)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(delimiter);

            return parser.Then(first =>
                delimiter
                    .IgnoreThen(parser)
                    .Many()
                    .Select(rest => ArrayEnumerable.Cons(first, rest))
            );
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="left"/>, discards the resulting value,
        /// then matches <paramref name="parser"/>, keeps the value, then matches <paramref name="right"/>
        /// and returns the value matched by <paramref name="parser"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="left">First parser to match, value is ignored.</param>
        /// <param name="right">Last parser to match, value is ignored.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T> Between<TOther>(TextParser<TOther> left, TextParser<TOther> right)
        {
            return left.IgnoreThen(parser.Then(right.Value));
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/>, discards the resulting value, then returns the result of <paramref name="second"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="second">The second parser.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<TOther> IgnoreThen<TOther>(TextParser<TOther> second)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(second);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return Result.CastEmpty<T, TOther>(rt);

                var ru = second(rt.Remainder);
                return !ru.HasValue ? ru : Result.Value(ru.Value, input, ru.Remainder);
            };
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/> zero or more times.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        /// <remarks>Many will fail if any item partially matches this. To modify this behavior use <see cref="Try{T}(TextParser{T})"/>.</remarks>
        public TextParser<T[]> Many()
        {
            ArgumentNullException.ThrowIfNull(parser);

            return input =>
            {
                var result = new List<T>();
                var from = input;
                var r = parser(input);
                while (r.HasValue)
                {
                    if (from == r.Remainder) // Broken parser, not a failed parsing.
                        throw new ParseException(
                            $"Many() cannot be applied to zero-width parsers; value {r.Value} at position {r.Location.Position}.",
                            r.Location.Position
                        );

                    result.Add(r.Value);

                    from = r.Remainder;
                    r = parser(r.Remainder);
                }

                if (!r.Backtrack && r.IsPartial(from))
                    return Result.CastEmpty<T, T[]>(r);

                return Result.Value(result.ToArray(), input, from);
            };
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/> zero or more times, discarding the
        /// result. This is useful for avoiding the array allocation performed by <see cref="Many{T}"/>.
        /// </summary>
        /// <returns>The resulting parser.</returns>
        /// <remarks>IgnoreMany will fail if any item partially matches this. To modify this behavior use <see cref="Try{T}(TextParser{T})"/>.</remarks>
        public TextParser<Unit> IgnoreMany()
        {
            ArgumentNullException.ThrowIfNull(parser);

            return input =>
            {
                var from = input;
                var r = parser(input);
                while (r.HasValue)
                {
                    if (from == r.Remainder) // Broken parser, not a failed parsing.
                        throw new ParseException(
                            $"IgnoreMany() cannot be applied to zero-width parsers; value {r.Value} at position {r.Location.Position}.",
                            r.Location.Position
                        );

                    from = r.Remainder;
                    r = parser(r.Remainder);
                }

                if (!r.Backtrack && r.IsPartial(from))
                    return Result.CastEmpty<T, Unit>(r);

                return Result.Value(Unit.Value, input, from);
            };
        }

        /// <summary>
        /// Construct a parser that matches <paramref name="parser"/> zero or more times, delimited by <paramref name="delimiter"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="delimiter">The parser that matches the delimiters.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T[]> ManyDelimitedBy<TOther>(TextParser<TOther> delimiter)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(delimiter);

            return parser
                .Then(first =>
                    delimiter
                        .IgnoreThen(parser)
                        .Many()
                        .Select(rest => ArrayEnumerable.Cons(first, rest))
                )
                .OptionalOrDefault([]);
        }

        /// <summary>
        /// Construct a parser that fails with error message <paramref name="errorMessage"/> when <paramref name="parser"/> fails.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T> Message(string errorMessage)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(errorMessage);

            return input =>
            {
                var result = parser(input);
                return result.HasValue ? result : Result.Empty<T>(result.Remainder, errorMessage);
            };
        }

        /// <summary>
        /// Construct a parser that returns <paramref name="name"/> as its "expectation" if <paramref name="parser"/> fails.
        /// </summary>
        /// <param name="name">The name given to <paramref name="parser"/>.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T> Named(string name)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(name);

            return input =>
            {
                var result = parser(input);
                if (result.HasValue || result.IsPartial(input))
                    return result;

                return Result.Empty<T>(result.Remainder, [name]);
            };
        }

        /// <summary>
        /// Construct a parser that matches zero or one instance of <paramref name="parser"/>, returning <paramref name="defaultValue"/> when
        /// no match is possible.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T> OptionalOrDefault(T defaultValue = default!)
        {
            ArgumentNullException.ThrowIfNull(parser);

            return parser.Or(Parse.Return(defaultValue!));
        }

        /// <summary>
        /// Construct a parser that tries first the <paramref name="parser"/> parser, and if it fails, applies <paramref name="rhs"/>.
        /// </summary>
        /// <param name="rhs">The second parser to try.</param>
        /// <returns>The resulting parser.</returns>
        /// <remarks>Or will fail if the first item partially matches this. To modify this behavior use <see cref="Try{T}(TextParser{T})"/>.</remarks>
        public TextParser<T> Or(TextParser<T> rhs)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(rhs);

            return input =>
            {
                var first = parser(input);
                if (first.HasValue || !first.Backtrack && first.IsPartial(input))
                    return first;

                var second = rhs(input);
                return second.HasValue ? second : Result.CombineEmpty(first, second);
            };
        }

        /// <summary>
        /// Construct a parser that takes the result of <paramref name="parser"/> and converts it value using <paramref name="selector"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="selector">A mapping from the first result to the second.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<TOther> Select<TOther>(Func<T, TOther> selector)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(selector);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return Result.CastEmpty<T, TOther>(rt);

                var u = selector(rt.Value);

                return Result.Value(u, input, rt.Remainder);
            };
        }

        /// <summary>
        /// The LINQ query comprehension pattern.
        /// </summary>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="selector">A mapping from the first result to the second parser.</param>
        /// <param name="projector">Function mapping the results of the first two parsers onto the final result.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<TOutput> SelectMany<TResult, TOutput>(
            Func<T, TextParser<TResult>> selector,
            Func<T, TResult, TOutput> projector
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(projector);

            return parser.Then(t => selector(t).Select(u => projector(t, u)));
        }

        /// <summary>
        /// Construct a parser that applies <paramref name="parser"/>, provides the value to <paramref name="second"/> and returns the result.
        /// </summary>
        /// <typeparam name="TOther">The type of the resulting value.</typeparam>
        /// <param name="second">The second parser.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<TOther> Then<TOther>(Func<T, TextParser<TOther>> second)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(second);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue)
                    return Result.CastEmpty<T, TOther>(rt);

                var ru = second(rt.Value)(rt.Remainder);
                return !ru.HasValue ? ru : Result.Value(ru.Value, input, ru.Remainder);
            };
        }
    }

    /// <summary>
    /// Construct a parser that matches zero or one instance of <paramref name="parser"/>.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, T?> Optional<TKind, T>(
        this TokenListParser<TKind, T> parser
    )
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(parser);

        return parser.Select(t => (T?)t).Or(Parse.Return<TKind, T?>(null));
    }

    /// <summary>
    /// Construct a parser that matches zero or one instance of <paramref name="parser"/>.
    /// </summary>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<T?> Optional<T>(this TextParser<T> parser)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(parser);

        return parser.Select(t => (T?)t).Or(Parse.Return<T?>(null));
    }

    /// <summary>
    /// Construct a parser that takes the result of <paramref name="parser"/> and casts it to <typeparamref name="TOther"/>.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <typeparam name="TOther">The type of the resulting value.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, TOther> Cast<TKind, T, TOther>(
        this TokenListParser<TKind, T> parser
    )
        where T : TOther
    {
        ArgumentNullException.ThrowIfNull(parser);

        return parser.Select(rt => (TOther)rt);
    }

    /// <summary>
    /// Construct a parser that takes the result of <paramref name="parser"/> and casts it to <typeparamref name="TOther"/>.
    /// </summary>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <typeparam name="TOther">The type of the resulting value.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<TOther> Cast<T, TOther>(this TextParser<T> parser)
        where T : TOther
    {
        ArgumentNullException.ThrowIfNull(parser);

        return parser.Select(rt => (TOther)rt);
    }

    /// <summary>
    /// Convert a parser of a non-null class type to its nullable equivalent.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, T?> AsNullable<TKind, T>(
        this TokenListParser<TKind, T> parser
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(parser);

        // ReSharper disable once RedundantCast
        return (TokenListParser<TKind, T?>)(object)parser;
    }

    /// <summary>
    /// Convert a parser of a non-null class type to its nullable equivalent.
    /// </summary>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<T?> AsNullable<T>(this TextParser<T> parser)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(parser);

        // ReSharper disable once RedundantCast
        return (TextParser<T?>)(object)parser;
    }

    /// <summary>
    /// Constructs a parser that converts a char[]-parser to a string-parser.
    /// </summary>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<string> Text(this TextParser<char[]> parser) =>
        parser.Select(chars => new string(chars));

    /// <summary>
    /// Constructs a parser that converts a TextSpan-parser to a string-parser.
    /// </summary>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<string> Text(this TextParser<TextSpan> parser) =>
        parser.Select(textSpan => textSpan.ToStringValue());

    /// <summary>
    /// Construct a parser that tries one parser, and backtracks if unsuccessful so that no input
    /// appears to have been consumed by subsequent checks against the result.
    /// </summary>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<T> Try<T>(this TextParser<T> parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        return input =>
        {
            var rt = parser(input);
            if (rt.HasValue)
                return rt;

            rt.Backtrack = true;
            return rt;
        };
    }

    /// <summary>
    /// Construct a parser that applies the first, and returns <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <typeparam name="TOther">The type of the resulting value.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <param name="value">The value to return.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, TOther> Value<TKind, T, TOther>(
        this TokenListParser<TKind, T> parser,
        TOther value
    )
    {
        ArgumentNullException.ThrowIfNull(parser);

        return parser.IgnoreThen(Parse.Return<TKind, TOther>(value));
    }

    /// <summary>
    /// Construct a parser that applies the first, and returns <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <typeparam name="TOther">The type of the resulting value.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <param name="value">The value to return.</param>
    /// <returns>The resulting parser.</returns>
    public static TextParser<TOther> Value<T, TOther>(this TextParser<T> parser, TOther value)
    {
        ArgumentNullException.ThrowIfNull(parser);

        return parser.IgnoreThen(Parse.Return(value));
    }

    /// <summary>
    /// Construct a parser that evaluates the result of a previous parser and fails if <paramref name="predicate"/> returns false
    /// for the result.
    /// </summary>
    /// <typeparam name="TKind">The kind of the tokens being parsed.</typeparam>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    /// <param name="parser">The parser.</param>
    /// <param name="predicate">The predicate to apply.</param>
    /// <param name="message">An optional error message when parsing fails.</param>
    /// <returns>The resulting parser.</returns>
    public static TokenListParser<TKind, T> Where<TKind, T>(
        this TokenListParser<TKind, T> parser,
        Func<T, bool> predicate,
        string message = "unsatisfied condition"
    )
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(message);

        return input =>
        {
            var rt = parser(input);
            if (!rt.HasValue || predicate(rt.Value))
                return rt;

            return TokenListParserResult.Empty<TKind, T>(input, message);
        };
    }

    /// <param name="parser">The parser.</param>
    /// <typeparam name="T">The type of value being parsed.</typeparam>
    extension<T>(TextParser<T> parser)
    {
        /// <summary>
        /// Construct a parser that evaluates the result of a previous parser and fails if <paramref name="predicate"/> returns false
        /// for the result.
        /// </summary>
        /// <param name="predicate">The predicate to apply.</param>
        /// <param name="message">An optional error message when parsing fails.</param>
        /// <returns>The resulting parser.</returns>
        public TextParser<T> Where(
            Func<T, bool> predicate,
            string message = "unsatisfied condition"
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(message);

            return input =>
            {
                var rt = parser(input);
                if (!rt.HasValue || predicate(rt.Value))
                    return rt;

                return Result.Empty<T>(input, message);
            };
        }

        /// <summary>
        /// Parse a sequence of operands connected by left-associative operators.
        /// </summary>
        /// <typeparam name="TOperator">The type of the operator.</typeparam>
        /// <typeparam name="TOperand">The type of subsequent operands.</typeparam>
        /// <param name="operator">A parser matching operators.</param>
        /// <param name="operand">A parser matching operands.</param>
        /// <param name="apply">A function combining the operator, left operand, and right operand, into the result.</param>
        /// <returns>The result of calling <paramref name="apply"/> successively on pairs of operands.</returns>
        public TextParser<T> Chain<TOperator, TOperand>(
            TextParser<TOperator> @operator,
            TextParser<TOperand> operand,
            Func<TOperator, T, TOperand, T> apply
        )
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(@operator);
            ArgumentNullException.ThrowIfNull(operand);
            ArgumentNullException.ThrowIfNull(apply);

            return input =>
            {
                var parseResult = parser(input);
                if (!parseResult.HasValue)
                    return parseResult;

                var result = parseResult.Value;
                var operandRemainder = parseResult.Remainder;

                var operatorResult = @operator(operandRemainder);
                while (operatorResult.HasValue || operatorResult.IsPartial(operandRemainder))
                {
                    // If operator read any input, but failed to read complete input, we return error
                    if (!operatorResult.HasValue)
                        return Result.CastEmpty<TOperator, T>(operatorResult);

                    var operandResult = operand(operatorResult.Remainder);
                    operandRemainder = operandResult.Remainder;

                    if (!operandResult.HasValue)
                        return Result.CastEmpty<TOperand, T>(operandResult);

                    result = apply(operatorResult.Value, result, operandResult.Value);

                    operatorResult = @operator(operandRemainder);
                }

                return Result.Value(result, input, operandRemainder);
            };
        }
    }
}
