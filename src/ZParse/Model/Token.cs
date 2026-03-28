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
/// A token.
/// </summary>
/// <typeparam name="TKind">The type of the token's kind.</typeparam>
public readonly struct Token<TKind>
{
    /// <summary>
    /// The kind of the token.
    /// </summary>
    public TKind Kind { get; }

    /// <summary>
    /// The string slice containing the value of the token.
    /// </summary>
    public TextSlice Slice { get; }

    /// <summary>
    /// The string span containing the value of the token.
    /// </summary>
    /// <param name="source">The source to get the span info from</param>
    /// <returns>The correct span</returns>
    public TextSpan Span(ReadOnlySpan<char> source) => Slice.AsTextSpan(source);

    /// <summary>
    /// Get the string value of the token.
    /// </summary>
    /// <returns>The token as a string.</returns>
    public string ToStringValue(ReadOnlySpan<char> source) => Span(source).ToStringValue();

    /// <summary>
    /// The position of the token within the source string.
    /// </summary>
    public Position Position => Slice.Position;

    /// <summary>
    /// True if the token has a value.
    /// </summary>
    public bool HasValue => Slice != TextSlice.None;

    /// <summary>
    /// Construct a token.
    /// </summary>
    /// <param name="kind">The kind of the token.</param>
    /// <param name="slice">The span holding the token's value.</param>
    public Token(TKind kind, TextSlice slice)
    {
        Kind = kind;
        Slice = slice;
    }

    /// <summary>
    /// A token with no value.
    /// </summary>
    public static Token<TKind> Empty => default;

    /// <inheritdoc/>
    public override string ToString()
    {
        return HasValue ? $"{Kind}@{Position}: {Slice}" : "(empty token)";
    }
}
