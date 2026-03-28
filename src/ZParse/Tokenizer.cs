// Copyright 2016-2018 Datalust, Superpower Contributors, Sprache Contributors
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

namespace ZParse;

/// <summary>
/// A tokenizer converts a string into a list of tokens.
/// </summary>
/// <typeparam name="TKind">The kind of tokens produced.</typeparam>
public interface ITokenizer<TKind>
{
    /// <summary>
    /// Tokenize <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source to tokenize.</param>
    /// <returns>The list of tokens or an error.</returns>
    /// <exception cref="ParseException">Tokenization failed.</exception>
    TokenList<TKind> Tokenize(ReadOnlySpan<char> source);

    /// <summary>
    /// Tokenize <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source to tokenize.</param>
    /// <returns>A result with the list of tokens or an error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="ParseException">The tokenizer could not correctly perform tokenization.</exception>
    Result<TokenList<TKind>> TryTokenize(ReadOnlySpan<char> source);
}

/// <summary>
/// An enumerator that produces tokens.
/// </summary>
/// <typeparam name="TKind">The kind of tokens produced.</typeparam>
public interface ITokenEnumerator<TKind> : IDisposable
{
    /// <summary>
    /// Advance to the next token.
    /// </summary>
    /// <param name="token">The token if found or default otherwise.</param>
    /// <returns>If a token was found.</returns>
    bool NextToken(out Result<TKind> token);
}

/// <summary>
/// Base class for tokenizers, types whose instances convert strings into lists of tokens.
/// </summary>
/// <typeparam name="TKind">The kind of tokens produced.</typeparam>
/// <typeparam name="TEnumerator">The type of enumerator used to produce tokens.</typeparam>
public abstract class Tokenizer<TKind, TEnumerator> : ITokenizer<TKind>
    where TEnumerator : ITokenEnumerator<TKind>, allows ref struct
{
    /// <inheritdoc />
    public TokenList<TKind> Tokenize(ReadOnlySpan<char> source)
    {
        var result = TryTokenize(source);
        return result.HasValue
            ? result.Value
            : throw new ParseException(result.ToString(), result.ErrorPosition);
    }

    /// <inheritdoc />
    public Result<TokenList<TKind>> TryTokenize(ReadOnlySpan<char> source)
    {
        var state = new TokenizationState<TKind>();

        var sourceSpan = new TextSpan(source);
        var remainder = sourceSpan;
        var results = new List<Token<TKind>>();
        using var enumerator = Tokenize(sourceSpan, state);
        while (enumerator.NextToken(out var result))
        {
            if (!result.HasValue)
                return Result.CastEmpty<TKind, TokenList<TKind>>(result);

            if (result.Remainder == remainder) // Broken parser, not a failed parsing.
                throw new ParseException(
                    $"Zero-width tokens are not supported; token {Presentation.FormatExpectation(result.Value)} at position {result.Location.Position}.",
                    result.Location.Position
                );

            remainder = result.Remainder;
            var token = new Token<TKind>(result.Value, result.Location.Until(result.Remainder));
            state.Previous = token;
            results.Add(token);
        }

        var value = new TokenList<TKind>(source, results.ToArray());
        return Result.Value(value, sourceSpan, remainder);
    }

    /// <summary>
    /// Subclasses should override to perform tokenization.
    /// </summary>
    /// <param name="span">The input span to tokenize.</param>
    /// <returns>A list of parsed tokens.</returns>
    protected virtual TEnumerator Tokenize(TextSpan span)
    {
        throw new NotImplementedException(
            "Either `Tokenize(TextSpan)` or `Tokenize(TextSpan, TokenizationState)` must be implemented."
        );
    }

    /// <summary>
    /// Subclasses should override to perform tokenization when the
    /// last-produced-token needs to be tracked.
    /// </summary>
    /// <param name="span">The input span to tokenize.</param>
    /// <param name="state">The tokenization state maintained during the operation.</param>
    /// <returns>A list of parsed tokens.</returns>
    protected virtual TEnumerator Tokenize(TextSpan span, TokenizationState<TKind> state)
    {
        return Tokenize(span);
    }

    /// <summary>
    /// Advance until the first non-whitespace character is encountered.
    /// </summary>
    /// <param name="span">The span to advance from.</param>
    /// <returns>A result with the first non-whitespace character.</returns>
    protected static Result<char> SkipWhiteSpace(TextSpan span)
    {
        var next = span.ConsumeChar();
        while (next.HasValue && char.IsWhiteSpace(next.Value))
        {
            next = next.Remainder.ConsumeChar();
        }
        return next;
    }
}
