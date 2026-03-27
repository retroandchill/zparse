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
using ZParse.Util;

namespace ZParse.Model;

/// <summary>
/// The result of parsing from a text span.
/// </summary>
/// <typeparam name="T">The type of the value being parsed.</typeparam>
public struct Result<T>
{
    /// <summary>
    /// If the result is a value, the location in the input corresponding to the
    /// value. If the result is an error, it's the location of the error.
    /// </summary>
    public TextSpan Location { get; }

    /// <summary>
    /// The first un-parsed location in the input.
    /// </summary>
    public TextSpan Remainder { get; }

    /// <summary>
    /// True if the result carries a successfully-parsed value; otherwise, false.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// If the result is an error, the source-level position of the error; otherwise, <see cref="Position.Empty"/>.
    /// </summary>
    public Position ErrorPosition => HasValue ? Position.Empty : Location.Position;

    /// <summary>
    /// A provided error message, or null.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// A list of expectations that were unmet, or null.
    /// </summary>
    public ImmutableArray<string> Expectations { get; }

    internal bool IsPartial(TextSpan from) => from != Remainder;

    internal bool Backtrack { get; set; }

    /// <summary>
    /// The parsed value.
    /// </summary>
    public T Value =>
        HasValue ? field : throw new InvalidOperationException($"{nameof(Result)} has no value.");

    internal Result(T value, TextSpan location, TextSpan remainder, bool backtrack)
    {
        Location = location;
        Remainder = remainder;
        Value = value;
        HasValue = true;
        ErrorMessage = null;
        Expectations = [];
        Backtrack = backtrack;
    }

    internal Result(
        TextSpan location,
        TextSpan remainder,
        string? errorMessage,
        ImmutableArray<string> expectations,
        bool backtrack
    )
    {
        Location = location;
        Remainder = remainder;
        Value = default!; // Default value is not observable.
        HasValue = false;
        Expectations = expectations;
        ErrorMessage = errorMessage;
        Backtrack = backtrack;
    }

    internal Result(
        TextSpan remainder,
        string? errorMessage,
        ImmutableArray<string> expectations,
        bool backtrack
    )
    {
        Location = Remainder = remainder;
        Value = default!; // Default value is not observable.
        HasValue = false;
        Expectations = expectations;
        ErrorMessage = errorMessage;
        Backtrack = backtrack;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Remainder == TextSpan.None)
            return "(Empty result.)";

        if (HasValue)
            return $"Successful parsing of {Value}.";

        var message = FormatErrorMessageFragment();
        var location = "";
        if (!Location.IsAtEnd)
        {
            location = $" (line {Location.Position.Line}, column {Location.Position.Column})";
        }

        return $"Syntax error{location}: {message}.";
    }

    /// <summary>
    /// If the result is empty, format the fragment of text describing the error.
    /// </summary>
    /// <returns>The error fragment.</returns>
    public string FormatErrorMessageFragment()
    {
        if (ErrorMessage != null)
            return ErrorMessage;

        string message;
        if (Location.IsAtEnd)
        {
            message = "unexpected end of input";
        }
        else
        {
            var next = Location.ConsumeChar().Value;
            message = $"unexpected {Display.Presentation.FormatLiteral(next)}";
        }

        if (Expectations.IsDefaultOrEmpty)
            return message;

        var expected = Friendly.List(Expectations);
        message += $", expected {expected}";

        return message;
    }
}

/// <summary>
/// Helper methods for working with <see cref="Result{T}"/>.
/// </summary>
public static class Result
{
    /// <summary>
    /// An empty result indicating no value could be parsed.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <returns>A result.</returns>
    public static Result<T> Empty<T>(TextSpan remainder)
    {
        return new Result<T>(remainder, null, [], false);
    }

    /// <summary>
    /// An empty result indicating no value could be parsed.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <param name="expectations">Literal descriptions of expectations not met.</param>
    /// <returns>A result.</returns>
    public static Result<T> Empty<T>(TextSpan remainder, ImmutableArray<string> expectations)
    {
        return new Result<T>(remainder, null, expectations, false);
    }

    /// <summary>
    /// An empty result indicating no value could be parsed.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <param name="errorMessage">Error message to present.</param>
    /// <returns>A result.</returns>
    public static Result<T> Empty<T>(TextSpan remainder, string errorMessage)
    {
        return new Result<T>(remainder, errorMessage, [], false);
    }

    /// <summary>
    /// A result carrying a successfully-parsed value.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="location">The location corresponding to the beginning of the parsed span.</param>
    /// <param name="remainder">The start of un-parsed input.</param>
    /// <returns>A result.</returns>
    public static Result<T> Value<T>(T value, TextSpan location, TextSpan remainder)
    {
        return new Result<T>(value, location, remainder, false);
    }

    /// <summary>
    /// Convert an empty result of one type into another.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TOther">The target type.</typeparam>
    /// <param name="result">The value to convert.</param>
    /// <returns>A result of type <typeparamref name="TOther"/> carrying the same information as <paramref name="result"/>.</returns>
    public static Result<TOther> CastEmpty<T, TOther>(Result<T> result)
    {
        return new Result<TOther>(
            result.Remainder,
            result.ErrorMessage,
            result.Expectations,
            result.Backtrack
        );
    }

    /// <summary>
    /// Combine two empty results.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <param name="first">The first value to combine.</param>
    /// <param name="second">The second value to combine.</param>
    /// <returns>A result of type <typeparamref name="T"/> carrying information from both results.</returns>
    public static Result<T> CombineEmpty<T>(Result<T> first, Result<T> second)
    {
        if (first.Remainder != second.Remainder)
            return second;

        var expectations = first.Expectations;
        if (expectations == null)
            expectations = second.Expectations;
        else if (second.Expectations != null)
            expectations = first.Expectations.AddRange(second.Expectations);

        return new Result<T>(second.Remainder, second.ErrorMessage, expectations, second.Backtrack);
    }
}
