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
using ZParse.Model;

namespace ZParse;

/// <summary>
/// A parser that consumes text from a string span.
/// </summary>
/// <typeparam name="T">The type of values produced by the parser.</typeparam>
/// <param name="input">The span of text to parse.</param>
/// <returns>A result with a parsed value, or an empty result indicating error.</returns>
public delegate Result<T> TextParser<T>(TextSpan input)
    where T : allows ref struct;

/// <summary>
/// Helper methods for working with parsers.
/// </summary>
public static class ParserExtensions
{
    /// <param name="parser">The parser.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    extension<T>(TextParser<T> parser)
        where T : allows ref struct
    {
        /// <summary>
        /// Tries to parse the input without throwing an exception upon failure.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the parser</returns>
        /// <exception cref="ArgumentNullException">The parser or input is null.</exception>
        public Result<T> TryParse(string input)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(input);

            return parser(new TextSpan(input));
        }

        /// <summary>
        /// Parses the specified input string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the parser.</returns>
        /// <exception cref="ArgumentNullException">The parser or input is null.</exception>
        /// <exception cref="ParseException">It contains the details of the parsing error.</exception>
        public T Parse(string input)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(input);

            var result = parser.TryParse(input);

            return result.HasValue
                ? result.Value
                : throw new ParseException(result.ToString(), result.ErrorPosition);
        }

        /// <summary>
        /// Tests whether the parser matches the entire provided <see cref="TextSpan"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>True if the parser is a complete match for the input; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">The parser is null.</exception>
        /// <exception cref="ArgumentException">The input is <see cref="TextSpan.Empty"/>.</exception>
        public bool IsMatch(TextSpan input)
        {
            ArgumentNullException.ThrowIfNull(parser);
            if (input == TextSpan.Empty)
                throw new ArgumentException("Input text span is empty.", nameof(input));

            var result = parser(input);
            return result is { HasValue: true, Remainder.IsAtEnd: true };
        }
    }

    /// <param name="parser">The parser.</param>
    /// <typeparam name="TKind">The type of tokens consumed by the parser.</typeparam>
    /// <typeparam name="T">The type of the result.</typeparam>
    extension<TKind, T>(TokenListParser<TKind, T> parser)
        where T : allows ref struct
    {
        /// <summary>
        /// Tries to parse the input without throwing an exception upon failure.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the parser</returns>
        /// <exception cref="ArgumentNullException">The parser or input is null.</exception>
        public TokenListParserResult<TKind, T> TryParse(TokenList<TKind> input)
        {
            ArgumentNullException.ThrowIfNull(parser);

            return parser(input);
        }

        /// <summary>
        /// Parses the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the parser.</returns>
        /// <exception cref="ArgumentNullException">The parser or input is null.</exception>
        /// <exception cref="ParseException">It contains the details of the parsing error.</exception>
        public T Parse(TokenList<TKind> input)
        {
            ArgumentNullException.ThrowIfNull(parser);

            var result = parser.TryParse(input);

            return result.HasValue
                ? result.Value
                : throw new ParseException(result.ToString(), result.ErrorPosition);
        }
    }
}
