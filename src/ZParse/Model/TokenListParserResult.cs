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
using System.Collections.Immutable;
using System.Linq;
using ZParse.Display;
using ZParse.Util;

namespace ZParse.Model;

/// <summary>
/// The result of parsing from a token list.
/// </summary>
/// <typeparam name="T">The type of the value being parsed.</typeparam>
/// <typeparam name="TKind">The kind of token being parsed.</typeparam>
public ref struct TokenListParserResult<TKind, T>
    where T : allows ref struct
{
    /// <summary>
    /// If the result has a value, this carries the location of the value in the token
    /// list. If the result is an error, it's the location of the error.
    /// </summary>
    public TokenList<TKind> Location { get; }

    /// <summary>
    /// The first un-parsed location in the list.
    /// </summary>
    public TokenList<TKind> Remainder { get; }

    /// <summary>
    /// True if the result carries a successfully-parsed value; otherwise, false.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// If the result is an error, the source-level position of the error; otherwise, <see cref="Position.Empty"/>.
    /// </summary>
    public Position ErrorPosition
    {
        get
        {
            if (HasValue)
                return Position.Empty;

            if (SubTokenErrorPosition.HasValue)
                return SubTokenErrorPosition;

            return !Remainder.IsAtEnd
                ? Remainder.ConsumeToken().Value.Position
                : Location.ComputeEndOfInputPosition(Location.Source);
        }
    }

    /// <summary>
    /// If the result is an error, the source-level position of the error; otherwise, <see cref="Position.Empty"/>.
    /// </summary>
    public Position SubTokenErrorPosition { get; }

    /// <summary>
    /// A provided error message, or null.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// A list of expectations that were unmet, or null.
    /// </summary>
    public ImmutableArray<string> Expectations { get; }

    /// <summary>
    /// The parsed value.
    /// </summary>
    public T Value =>
        HasValue
            ? field
            : throw new InvalidOperationException($"{nameof(TokenListParserResult)} has no value.");

    internal bool IsPartial(TokenList<TKind> from) =>
        SubTokenErrorPosition.HasValue || from != Remainder;

    internal bool Backtrack { get; set; }

    internal TokenListParserResult(
        T value,
        TokenList<TKind> location,
        TokenList<TKind> remainder,
        bool backtrack
    )
    {
        Location = location;
        Remainder = remainder;
        Value = value;
        HasValue = true;
        SubTokenErrorPosition = Position.Empty;
        ErrorMessage = null;
        Expectations = [];
        Backtrack = backtrack;
    }

    internal TokenListParserResult(
        TokenList<TKind> location,
        TokenList<TKind> remainder,
        Position errorPosition,
        string? errorMessage,
        ImmutableArray<string> expectations,
        bool backtrack
    )
    {
        Location = location;
        Remainder = remainder;
        Value = default!; // Default value is not observable.
        HasValue = false;
        SubTokenErrorPosition = errorPosition;
        ErrorMessage = errorMessage;
        Expectations = expectations;
        Backtrack = backtrack;
    }

    internal TokenListParserResult(
        TokenList<TKind> remainder,
        Position errorPosition,
        string? errorMessage,
        ImmutableArray<string> expectations,
        bool backtrack
    )
    {
        Location = Remainder = remainder;
        Value = default!; // Default value is not observable.
        HasValue = false;
        SubTokenErrorPosition = errorPosition;
        ErrorMessage = errorMessage;
        Expectations = expectations;
        Backtrack = backtrack;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Remainder == TokenList<TKind>.Empty)
            return "(Empty result.)";

        if (HasValue)
        {
            var asString = Result<T>.Stringify?.Invoke(Value);
            return asString is not null
                ? $"Successful parsing of {asString}."
                : "Successful parsing.";
        }

        var message = FormatErrorMessageFragment();
        var location = "";
        if (!Remainder.IsAtEnd)
        {
            // Since the message notes `end of input`, don't report line/column here.
            var sourcePosition = SubTokenErrorPosition.HasValue
                ? SubTokenErrorPosition
                : Remainder.ConsumeToken().Value.Position;
            location = $" (line {sourcePosition.Line}, column {sourcePosition.Column})";
        }

        return $"Syntax error{location}: {message}.";
    }

    /// <summary>
    /// If the result is empty, format the fragment of text describing the error.
    /// </summary>
    /// <returns>The error fragment.</returns>
    public string FormatErrorMessageFragment()
    {
        if (ErrorMessage is not null)
            return ErrorMessage;

        string message;
        if (Remainder.IsAtEnd)
        {
            message = "unexpected end of input";
        }
        else
        {
            var next = Remainder.ConsumeToken().Value;
            var appearance = Presentation.FormatAppearance(
                next.Kind,
                next.ToStringValue(Location.Source)
            );
            message = $"unexpected {appearance}";
        }

        if (Expectations.IsDefaultOrEmpty)
            return message;

        var expected = Friendly.List(Expectations);
        message += $", expected {expected}";

        return message;
    }
}

/// <summary>
/// Helper methods for working with <see cref="TokenListParserResult{TKind,T}"/>.
/// </summary>
public static class TokenListParserResult
{
    /// <summary>
    /// Create a token result with no value, indicating a failure to parse any value.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <returns>An empty result.</returns>
    public static TokenListParserResult<TKind, T> Empty<TKind, T>(TokenList<TKind> remainder)
        where T : allows ref struct
    {
        return new TokenListParserResult<TKind, T>(remainder, Position.Empty, null, [], false);
    }

    /// <summary>
    /// Create a token result with no value, indicating a failure to parse any value.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <param name="expectations">Expectations that could not be fulfilled.</param>
    /// <returns>An empty result.</returns>
    public static TokenListParserResult<TKind, T> Empty<TKind, T>(
        TokenList<TKind> remainder,
        ImmutableArray<string> expectations
    )
        where T : allows ref struct
    {
        return new TokenListParserResult<TKind, T>(
            remainder,
            Position.Empty,
            null,
            expectations,
            false
        );
    }

    /// <summary>
    /// Create a token result with no value, indicating a failure to parse any value.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <param name="expectations">Expectations that could not be fulfilled.</param>
    /// <returns>An empty result.</returns>
    public static TokenListParserResult<TKind, T> Empty<TKind, T>(
        TokenList<TKind> remainder,
        TKind[] expectations
    )
        where T : allows ref struct
    {
        var stringExpectations = expectations
            .Select(Presentation.FormatExpectation)
            .ToImmutableArray();
        return new TokenListParserResult<TKind, T>(
            remainder,
            Position.Empty,
            null,
            stringExpectations,
            false
        );
    }

    /// <summary>
    /// Create a token result with no value, indicating a failure to parse any value.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <param name="errorMessage">An error message describing why the tokens could not be parsed.</param>
    /// <returns>An empty result.</returns>
    public static TokenListParserResult<TKind, T> Empty<TKind, T>(
        TokenList<TKind> remainder,
        string errorMessage
    )
        where T : allows ref struct
    {
        return new TokenListParserResult<TKind, T>(
            remainder,
            Position.Empty,
            errorMessage,
            [],
            false
        );
    }

    /// <summary>
    /// Create a token result with no value, indicating a failure to parse any value.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <param name="errorPosition">A source position within an individual token where parsing failed. In this case the position will be within
    /// the first token in <paramref name="remainder"/>.</param>
    /// <param name="errorMessage">A message describing the problem.</param>
    /// <returns>An empty result.</returns>
    public static TokenListParserResult<TKind, T> Empty<TKind, T>(
        TokenList<TKind> remainder,
        Position errorPosition,
        string errorMessage
    )
        where T : allows ref struct
    {
        return new TokenListParserResult<TKind, T>(
            remainder,
            errorPosition,
            errorMessage,
            [],
            false
        );
    }

    /// <summary>
    /// Create a token result with the provided value.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="location">The location where parsing began.</param>
    /// <param name="remainder">The first un-parsed location.</param>
    /// <returns></returns>
    public static TokenListParserResult<TKind, T> Value<TKind, T>(
        T value,
        TokenList<TKind> location,
        TokenList<TKind> remainder
    )
        where T : allows ref struct
    {
        return new TokenListParserResult<TKind, T>(value, location, remainder, false);
    }

    /// <summary>
    /// Convert an empty result of one type into another.
    /// </summary>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TOther">The destination type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>The converted result.</returns>
    public static TokenListParserResult<TKind, TOther> CastEmpty<TKind, T, TOther>(
        TokenListParserResult<TKind, T> result
    )
        where T : allows ref struct
        where TOther : allows ref struct
    {
        return new TokenListParserResult<TKind, TOther>(
            result.Remainder,
            result.SubTokenErrorPosition,
            result.ErrorMessage,
            result.Expectations,
            result.Backtrack
        );
    }

    /// <summary>
    /// Combine two empty results.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TKind">The kind of token.</typeparam>
    /// <param name="first">The first value to combine.</param>
    /// <param name="second">The second value to combine.</param>
    /// <returns>A result of type <typeparamref name="T"/> carrying information from both results.</returns>
    public static TokenListParserResult<TKind, T> CombineEmpty<TKind, T>(
        TokenListParserResult<TKind, T> first,
        TokenListParserResult<TKind, T> second
    )
        where T : allows ref struct
    {
        if (first.Remainder != second.Remainder)
            return second;

        var expectations = first.Expectations;
        if (expectations.IsDefaultOrEmpty)
            expectations = second.Expectations;
        else if (!second.Expectations.IsDefaultOrEmpty)
        {
            var expectationsBuilder = ImmutableArray.CreateBuilder<string>(
                first.Expectations.Length + second.Expectations.Length
            );
            var i = 0;
            for (; i < first.Expectations.Length; ++i)
                expectationsBuilder.Add(first.Expectations[i]);
            for (var j = 0; j < second.Expectations.Length; ++i, ++j)
                expectationsBuilder.Add(second.Expectations[j]);

            expectations = expectationsBuilder.DrainToImmutable();
        }

        return new TokenListParserResult<TKind, T>(
            second.Remainder,
            second.SubTokenErrorPosition,
            first.ErrorMessage,
            expectations,
            second.Backtrack
        );
    }
}
