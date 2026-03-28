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

namespace ZParse.Model;

/// <summary>
/// Represents a span of text within a larger string.
/// </summary>
public readonly record struct TextSlice
{
    /// <summary>
    /// The position of the start of the span within the string.
    /// </summary>
    public Position Position { get; init; }

    /// <summary>
    /// The length of the span.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Construct a slice with the given parameters
    /// </summary>
    /// <param name="position">The start of the span.</param>
    /// <param name="length">The length of the span.</param>
    public TextSlice(Position position, int length)
    {
#if CHECKED
        if (length < 0)
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "The length must be non-negative."
            );
#endif

        Position = position;
        Length = length;
    }

    /// <summary>
    /// Construct a slice that corresponds to the given span.
    /// </summary>
    /// <param name="span">The span to get the slice parameters from.</param>
    public TextSlice(TextSpan span)
        : this(span.Position, span.Length) { }

    /// <summary>
    /// Implicit conversion from a span to a slice.
    /// </summary>
    /// <param name="span">The span to get the slice parameters from.</param>
    /// <returns>The constructed slice.</returns>
    public static implicit operator TextSlice(TextSpan span) => new(span);

    /// <summary>
    /// A span with no value.
    /// </summary>
    public static TextSlice None => default;

    /// <summary>
    /// A span corresponding to the empty string.
    /// </summary>
    public static TextSlice Empty { get; } = new(Position.Zero, 0);

    /// <summary>
    /// Convert the slice to a text span.
    /// </summary>
    /// <param name="source">The source span to construct from</param>
    /// <returns>The constructed pan</returns>
    public TextSpan AsTextSpan(ReadOnlySpan<char> source) => new(source, Position, Length);

    /// <inheritdoc />
    public override string ToString()
    {
        return $"TextSlice(Position={Position}, Length={Length})";
    }
}
