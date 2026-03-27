// Copyright 2018 Datalust, Superpower Contributors, Sprache Contributors
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
using System.Collections.Immutable;
using ZParse.Display;
using ZParse.Model;

namespace ZParse.Tokenizers;

/// <summary>
/// Builds a simple tokenizer given information about tokens and whitespace.
/// </summary>
/// <remarks>Provides a quick way to get started with a simple
/// tokenizer that matches the input against a list of possible token
/// recognizers.</remarks>
/// <typeparam name="TKind">The kind of token the tokenizer will
/// produce.</typeparam>
public class TokenizerBuilder<TKind>
{
    private readonly record struct Recognizer(
        TextParser<Unit> Parser,
        bool IsIgnored,
        TKind Kind,
        bool IsDelimiter
    );

    private readonly List<Recognizer> _recognizers = [];

    /// <summary>
    /// Add a recognizer for a kind of token. Recognizers are tried in the order
    /// in which they are added.
    /// </summary>
    /// <param name="recognizer">A parser that will recognize the token.</param>
    /// <param name="kind">The kind of token the recognizer will recognize.</param>
    /// <param name="requireDelimiters">If true, the token must be preceded and followed
    /// by either the beginning or end-of-input, an ignored (whitespace) character,
    /// or a token kind that does not require delimiters. Generally set to `true` for
    /// keywords/identifiers, otherwise, use the default value of `false`.</param>
    /// <typeparam name="T">The value produced by the recognizer, if any. This
    /// will be ignored.</typeparam>
    /// <returns>The builder, to allow method chaining.</returns>
    public TokenizerBuilder<TKind> Match<T>(
        TextParser<T> recognizer,
        TKind kind,
        bool requireDelimiters = false
    )
    {
        ArgumentNullException.ThrowIfNull(recognizer);
        _recognizers.Add(
            new Recognizer(recognizer.Value(Unit.Value), false, kind, !requireDelimiters)
        );
        return this;
    }

    /// <summary>
    /// Add a recognizer for a whitespace/ignored text.
    /// </summary>
    /// <param name="ignored">A recognizer for the ignored text.</param>
    /// <typeparam name="T">The value produced by the recognizer, if any. This
    /// will be ignored.</typeparam>
    /// <returns>The builder, to allow method chaining.</returns>
    public TokenizerBuilder<TKind> Ignore<T>(TextParser<T> ignored)
    {
        ArgumentNullException.ThrowIfNull(ignored);
        _recognizers.Add(new Recognizer(ignored.Value(Unit.Value), true, default!, true));
        return this;
    }

    /// <summary>
    /// Build the tokenizer.
    /// </summary>
    /// <returns>The tokenizer.</returns>
    public Tokenizer<TKind> Build()
    {
        return new SimpleLinearTokenizer(_recognizers);
    }

    private class SimpleLinearTokenizer : Tokenizer<TKind>
    {
        private readonly ImmutableArray<Recognizer> _recognizers;

        public SimpleLinearTokenizer(IEnumerable<Recognizer> recognizers)
        {
            ArgumentNullException.ThrowIfNull(recognizers);
            _recognizers = [.. recognizers];
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The complexity in this method is due to the desire to distinguish between (e.g. in C#)
        /// the keyword `null` vs the identifier `nullability`. The tokenizer, when it encounters
        /// a non-delimiter match (like `null`), looks ahead to see whether it's immediately followed
        /// by a delimiter or end-of-input. If not, the match is discarded and subsequent recognizers
        /// are tested.
        /// </remarks>
        protected override IEnumerable<Result<TKind>> Tokenize(TextSpan span)
        {
            var remainder = span;
            var current = default(Result<TKind>);
            var recognizerSearchStart = 0;
            var recognizerIndex = -1;
            var hasCurrent = false;

            while (
                hasCurrent
                || TryMatch(remainder, recognizerSearchStart, out current, out recognizerIndex)
            )
            {
                var recognizer = _recognizers[recognizerIndex];
                if (recognizer.IsIgnored)
                {
                    remainder = current.Remainder;
                    hasCurrent = false;
                    current = default;
                    recognizerSearchStart = 0;
                    recognizerIndex = -1;
                }
                else if (recognizer.IsDelimiter || current.Remainder.IsAtEnd)
                {
                    yield return current;
                    remainder = current.Remainder;
                    hasCurrent = false;
                    current = default;
                    recognizerSearchStart = 0;
                    recognizerIndex = -1;
                }
                else if (
                    TryMatch(current.Remainder, 0, out var next, out var nextRecognizerIndex)
                    && _recognizers[nextRecognizerIndex].IsDelimiter
                )
                {
                    yield return current;
                    hasCurrent = true;
                    current = next;
                    remainder = current.Remainder;
                    recognizerSearchStart = 0;
                    recognizerIndex = nextRecognizerIndex;
                }
                else if (recognizerIndex < _recognizers.Length - 1)
                {
                    hasCurrent = false;
                    current = default;
                    recognizerSearchStart = recognizerIndex + 1;
                    recognizerIndex = -1;
                }
                else
                {
                    break;
                }
            }

            if (remainder.IsAtEnd)
                yield break;

            // Even though this re-runs all of the recognizers, it's better for performance
            // to calculate the error here, than do all of the extra work in the hot/success path.
            var failure = Result.Empty<TKind>(remainder);
            foreach (var recognizer in _recognizers)
            {
                var attempt = recognizer.Parser(remainder);
                if (
                    !attempt.HasValue
                    && // <- Successful recognizers rejected because delimiters were not present
                    attempt.ErrorPosition.Absolute > failure.ErrorPosition.Absolute
                )
                {
                    // We know the token's kind here, so might as well included it so that we can yield more
                    // detailed messages. Reporting the failure position as the token's start position makes it
                    // much more sensible to refer to the token by kind, and easier to figure out what's going on
                    // in cases like missing closing delimiters (which end pulling the whole remainder into the
                    // token). Including the actual failure position in the error message helps to further pinpoint
                    // the problem.
                    var problem = attempt.Remainder.IsAtEnd ? "incomplete" : "invalid";
                    var augmentedMessage =
                        $"{problem} {Presentation.FormatExpectation(recognizer.Kind)}, {attempt.FormatErrorMessageFragment()}";
                    if (!attempt.Remainder.IsAtEnd)
                        augmentedMessage +=
                            $" at line {attempt.Remainder.Position.Line}, column {attempt.Remainder.Position.Column}";
                    failure = new Result<TKind>(
                        remainder,
                        augmentedMessage,
                        attempt.Expectations,
                        attempt.Backtrack
                    );
                }
            }

            yield return failure;
        }

        private bool TryMatch(
            TextSpan span,
            int searchStart,
            out Result<TKind> match,
            out int recognizerIndex
        )
        {
            if (!span.IsAtEnd)
            {
                while (searchStart < _recognizers.Length)
                {
                    var recognizer = _recognizers[searchStart];
                    var attempt = recognizer.Parser(span);
                    if (attempt.HasValue)
                    {
                        if (attempt.Remainder == span) // Broken parser, not a failed parsing.
                            throw new ParseException(
                                $"Zero-width tokens are not supported; token {Presentation.FormatExpectation(recognizer.Kind)} at position {attempt.Location.Position}.",
                                attempt.Location.Position
                            );

                        match = Result.Value(recognizer.Kind, span, attempt.Remainder);
                        recognizerIndex = searchStart;
                        return true;
                    }

                    searchStart++;
                }
            }

            match = default;
            recognizerIndex = -1;
            return false;
        }
    }
}
