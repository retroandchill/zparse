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
using System.Collections;
using System.Collections.Generic;
using ZLinq;
using ZLinq.Internal;

namespace ZParse.Model;

/// <summary>
/// A span of text within a larger string.
/// </summary>
public readonly ref struct TextSpan : IEquatable<TextSpan>
{
    private const string CannotBeBoxed =
        $"{nameof(TextSpan)} is a ref struct, and thus cannot be boxed.";

    /// <summary>
    /// The source string containing the span.
    /// </summary>
    public ReadOnlySpan<char> Source { get; }

    /// <summary>
    /// The position of the start of the span within the string.
    /// </summary>
    public Position Position { get; }

    /// <summary>
    /// The length of the span.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Construct a span encompassing an entire string.
    /// </summary>
    /// <param name="source">The source string.</param>
    public TextSpan(ReadOnlySpan<char> source)
        : this(source, Position.Zero, source.Length) { }

    /// <summary>
    /// Construct a string span for a substring of <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="position">The start of the span.</param>
    /// <param name="length">The length of the span.</param>
    public TextSpan(ReadOnlySpan<char> source, Position position, int length)
    {
#if CHECKED
        if (length < 0)
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "The length must be non-negative."
            );
        if (source.Length < position.Absolute + length)
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "The token extends beyond the end of the input."
            );
#endif

        Source = source;
        Position = position;
        Length = length;
    }

    /// <summary>
    /// A span with no value.
    /// </summary>
    public static TextSpan None => default;

    /// <summary>
    /// A span corresponding to the empty string.
    /// </summary>
    public static TextSpan Empty => new([], Position.Zero, 0);

    /// <summary>
    /// True if the span has no content.
    /// </summary>
    public bool IsAtEnd
    {
        get
        {
            EnsureHasValue();
            return Length == 0;
        }
    }

    private bool IsValid => Position != Position.Empty;

    private void EnsureHasValue()
    {
        if (!IsValid)
            throw new InvalidOperationException("String span has no value.");
    }

    /// <summary>
    /// Consume a character from the start of the span.
    /// </summary>
    /// <returns>A result with the character and remainder.</returns>
    public Result<char> ConsumeChar()
    {
        EnsureHasValue();

        if (IsAtEnd)
            return Result.Empty<char>(this);

        var ch = Source![Position.Absolute];
        return Result.Value(ch, this, new TextSpan(Source, Position.Advance(ch), Length - 1));
    }

    /// <summary>
    /// This method is not supported as views cannot be boxed. To compare two views, use operator==.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    public override bool Equals(object? obj)
    {
        throw new NotSupportedException(CannotBeBoxed);
    }

    /// <summary>
    /// This method is not supported as views cannot be boxed. To compare two views, use operator==.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    public override int GetHashCode()
    {
        throw new NotSupportedException(CannotBeBoxed);
    }

    /// <summary>
    /// Compare a string span with another using source identity
    /// semantics - same source, same position, same length.
    /// </summary>
    /// <param name="other">The other span.</param>
    /// <returns>True if the spans are the same.</returns>
    public bool Equals(TextSpan other)
    {
        return Source == other.Source
            && Position.Absolute == other.Position.Absolute
            && Length == other.Length;
    }

    /// <summary>
    /// Compare two spans using source identity semantics.
    /// </summary>
    /// <param name="lhs">One span.</param>
    /// <param name="rhs">Another span.</param>
    /// <returns>True if the spans are the same.</returns>
    public static bool operator ==(TextSpan lhs, TextSpan rhs)
    {
        return lhs.Equals(rhs);
    }

    /// <summary>
    /// Compare two spans using source identity semantics.
    /// </summary>
    /// <param name="lhs">One span.</param>
    /// <param name="rhs">Another span.</param>
    /// <returns>True if the spans are the different.</returns>
    public static bool operator !=(TextSpan lhs, TextSpan rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Return a new span from the start of this span to the beginning of another.
    /// </summary>
    /// <param name="next">The next span.</param>
    /// <returns>A sub-span.</returns>
    public TextSpan Until(TextSpan next)
    {
#if CHECKED
        next.EnsureHasValue();
        if (next.Source != Source)
            throw new ArgumentException("The spans are on different source strings.", nameof(next));
#endif
        var charCount = next.Position.Absolute - Position.Absolute;
        return First(charCount);
    }

    /// <summary>
    /// Return a span comprising the first <paramref name="length"/> characters of this span.
    /// </summary>
    /// <param name="length">The number of characters to return.</param>
    /// <returns>The sub-span.</returns>
    public TextSpan First(int length)
    {
#if CHECKED
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (length > Length)
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "Length exceeds the source span's length."
            );
#endif

        return new TextSpan(Source, Position, length);
    }

    /// <summary>
    /// Skip a specified number of characters. Note, this is an O(count) operation.
    /// </summary>
    /// <param name="count"></param>
    public TextSpan Skip(int count)
    {
        EnsureHasValue();

#if CHECKED
        if (count > Length)
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Count exceeds the source span's length."
            );
#endif

        var p = Position;
        for (var i = 0; i < count; ++i)
        {
            p = p.Advance(Source[p.Absolute]);
        }

        return new TextSpan(Source, p, Length - count);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return IsValid ? ToStringValue() : "(empty source span)";
    }

    /// <summary>
    /// Compute the string value of this span.
    /// </summary>
    /// <returns>A string with the value of this span.</returns>
    public string ToStringValue()
    {
        EnsureHasValue();
        return AsReadOnlySpan().ToString();
    }

    /// <summary>
    /// Compute the string value of this span and return the view of source directly.
    /// </summary>
    /// <returns>A view of string with the value of this span.</returns>
    public ReadOnlySpan<char> AsReadOnlySpan()
    {
        EnsureHasValue();
        return Source.Slice(Position.Absolute, Length);
    }

    /// <summary>
    /// Compare the contents of this span with <paramref name="otherValue"/>.
    /// </summary>
    /// <param name="otherValue">The string value to compare.</param>
    /// <returns>True if the values are the same.</returns>
    public bool EqualsValue(ReadOnlySpan<char> otherValue)
    {
        EnsureHasValue();
        return AsReadOnlySpan().Equals(otherValue, StringComparison.Ordinal);
    }

    /// <summary>
    /// Compare the contents of this span with <paramref name="otherValue"/>, ignoring invariant character case.
    /// </summary>
    /// <param name="otherValue">The string value to compare.</param>
    /// <returns>True if the values are the same ignoring case.</returns>
    public bool EqualsValueIgnoreCase(ReadOnlySpan<char> otherValue)
    {
        EnsureHasValue();
        return AsReadOnlySpan().Equals(otherValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the character at the specified index in the text span.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the character to get.
    /// </param>
    /// <returns>
    /// The character at the specified index in the text span.
    /// </returns>
    public char this[int index]
    {
        get
        {
            EnsureHasValue();
#if CHECKED
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if ((uint)index >= (uint)Length)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index exceeds the source span's length."
                );
#endif
            return Source[Position.Absolute + index];
        }
    }

    /// <summary>
    /// Forms a slice out of the current text span starting at the specified index.
    /// </summary>
    /// <param name="index">
    /// The index at which to begin the slice.
    /// </param>
    /// <returns>
    /// An text span that consists of all elements of the current array segment from <paramref name="index"/> to the end of the text span.
    /// </returns>
    public TextSpan Slice(int index)
    {
        return Skip(index);
    }

    /// <summary>
    /// Forms a slice of the specified length out of the current text span starting at the specified index.
    /// </summary>
    /// <param name="index">The index at which to begin the slice.</param>
    /// <param name="count">The desired length of the slice.</param>
    /// <returns>An text span of <paramref name="count"/> elements starting at <paramref name="index"/>.</returns>
    public TextSpan Slice(int index, int count)
    {
        return Skip(index).First(count);
    }

    /// <summary>
    /// Get an enumerator for the view.
    /// </summary>
    /// <returns>An enumerator to iterate over the view</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Gets a ZLinq compatible value enumerable for the view.
    /// </summary>
    /// <returns>The value enumerable for the view.</returns>
    public ValueEnumerable<Enumerator, char> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, char>(GetEnumerator());
    }

    /// <summary>
    /// Enumerator for <see cref="TextSpan"/>.
    /// </summary>
    /// <param name="owner">The view to enumerate.</param>
    public ref struct Enumerator(TextSpan owner) : IEnumerator<char>, IValueEnumerator<char>
    {
        private readonly TextSpan _owner = owner;

        private int _index;

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public char Current => _owner[_index];

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_index >= _owner.Length)
                return false;

            _index++;
            return _index < _owner.Length;
        }

        /// <inheritdoc />
        public bool TryGetNext(out char current)
        {
            if (MoveNext())
            {
                current = Current;
                return true;
            }

            current = '\0';
            return false;
        }

        /// <inheritdoc />
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _owner.Length;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetSpan(out ReadOnlySpan<char> span)
        {
            span = _owner.AsReadOnlySpan();
            return true;
        }

        /// <inheritdoc />
        public bool TryCopyTo(scoped Span<char> destination, Index offset)
        {
            if (
                !EnumeratorHelper.TryGetSlice(
                    _owner.AsReadOnlySpan(),
                    offset,
                    destination.Length,
                    out var slice
                )
            )
                return false;

            slice.CopyTo(destination);
            return true;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _index = -1;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // No resources to dispose of
        }
    }
}
