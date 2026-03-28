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

namespace ZParse.Model;

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
public static class RefTuple
{
    /// <summary>
    /// Create a new <see cref="RefTuple{T1}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1> Create<T1>(T1 item1)
        where T1 : allows ref struct
    {
        return new RefTuple<T1>(item1);
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        return new RefTuple<T1, T2>(item1, item2);
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2,T3}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <typeparam name="T3">The type of the third item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
    {
        return new RefTuple<T1, T2, T3>(item1, item2, item3);
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2,T3,T4}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <typeparam name="T3">The type of the third item</typeparam>
    /// <typeparam name="T4">The type of the fourth item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(
        T1 item1,
        T2 item2,
        T3 item3,
        T4 item4
    )
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
        where T4 : allows ref struct
    {
        return new RefTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2,T3,T4,T5}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <typeparam name="T3">The type of the third item</typeparam>
    /// <typeparam name="T4">The type of the fourth item</typeparam>
    /// <typeparam name="T5">The type of the fifth item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(
        T1 item1,
        T2 item2,
        T3 item3,
        T4 item4,
        T5 item5
    )
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
        where T4 : allows ref struct
        where T5 : allows ref struct
    {
        return new RefTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2,T3,T4,T5,T6}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <param name="item6">The sixth item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <typeparam name="T3">The type of the third item</typeparam>
    /// <typeparam name="T4">The type of the fourth item</typeparam>
    /// <typeparam name="T5">The type of the fifth item</typeparam>
    /// <typeparam name="T6">The type of the sixth item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(
        T1 item1,
        T2 item2,
        T3 item3,
        T4 item4,
        T5 item5,
        T6 item6
    )
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
        where T4 : allows ref struct
        where T5 : allows ref struct
        where T6 : allows ref struct
    {
        return new RefTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2,T3,T4,T5,T6,T7}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <param name="item6">The sixth item of the tuple</param>
    /// <param name="item7">The seventh item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <typeparam name="T3">The type of the third item</typeparam>
    /// <typeparam name="T4">The type of the fourth item</typeparam>
    /// <typeparam name="T5">The type of the fifth item</typeparam>
    /// <typeparam name="T6">The type of the sixth item</typeparam>
    /// <typeparam name="T7">The type of the seventh item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(
        T1 item1,
        T2 item2,
        T3 item3,
        T4 item4,
        T5 item5,
        T6 item6,
        T7 item7
    )
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
        where T4 : allows ref struct
        where T5 : allows ref struct
        where T6 : allows ref struct
        where T7 : allows ref struct
    {
        return new RefTuple<T1, T2, T3, T4, T5, T6, T7>(
            item1,
            item2,
            item3,
            item4,
            item5,
            item6,
            item7
        );
    }

    /// <summary>
    /// Create a new <see cref="RefTuple{T1,T2,T3,T4,T5,T6,T7,T8}"/> with the specified items.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <param name="item6">The sixth item of the tuple</param>
    /// <param name="item7">The seventh item of the tuple</param>
    /// <param name="item8">The eighth item of the tuple</param>
    /// <typeparam name="T1">The type of the first item</typeparam>
    /// <typeparam name="T2">The type of the second item</typeparam>
    /// <typeparam name="T3">The type of the third item</typeparam>
    /// <typeparam name="T4">The type of the fourth item</typeparam>
    /// <typeparam name="T5">The type of the fifth item</typeparam>
    /// <typeparam name="T6">The type of the sixth item</typeparam>
    /// <typeparam name="T7">The type of the seventh item</typeparam>
    /// <typeparam name="T8">The type of the eighth item</typeparam>
    /// <returns>The created tuple</returns>
    public static RefTuple<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        T1 item1,
        T2 item2,
        T3 item3,
        T4 item4,
        T5 item5,
        T6 item6,
        T7 item7,
        T8 item8
    )
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
        where T4 : allows ref struct
        where T5 : allows ref struct
        where T6 : allows ref struct
        where T7 : allows ref struct
        where T8 : allows ref struct
    {
        return new RefTuple<T1, T2, T3, T4, T5, T6, T7, T8>(
            item1,
            item2,
            item3,
            item4,
            item5,
            item6,
            item7,
            item8
        );
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
public readonly ref struct RefTuple<T1>(T1 item1)
    where T1 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    public void Deconstruct(out T1 item1) => item1 = Item1;
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
public readonly ref struct RefTuple<T1, T2>(T1 item1, T2 item2)
    where T1 : allows ref struct
    where T2 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    public void Deconstruct(out T1 item1, out T2 item2)
    {
        item1 = Item1;
        item2 = Item2;
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <param name="item3">The third item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
/// <typeparam name="T3">The type of the third item</typeparam>
public readonly ref struct RefTuple<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where T3 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// The third item of the tuple.
    /// </summary>
    public T3 Item3 { get; } = item3;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    public void Deconstruct(out T1 item1, out T2 item2, out T3 item3)
    {
        item1 = Item1;
        item2 = Item2;
        item3 = Item3;
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <param name="item3">The third item of the tuple</param>
/// <param name="item4">The fourth item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
/// <typeparam name="T3">The type of the third item</typeparam>
/// <typeparam name="T4">The type of the fourth item</typeparam>
public readonly ref struct RefTuple<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where T3 : allows ref struct
    where T4 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// The third item of the tuple.
    /// </summary>
    public T3 Item3 { get; } = item3;

    /// <summary>
    /// The fourth item of the tuple.
    /// </summary>
    public T4 Item4 { get; } = item4;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4)
    {
        item1 = Item1;
        item2 = Item2;
        item3 = Item3;
        item4 = Item4;
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <param name="item3">The third item of the tuple</param>
/// <param name="item4">The fourth item of the tuple</param>
/// <param name="item5">The fifth item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
/// <typeparam name="T3">The type of the third item</typeparam>
/// <typeparam name="T4">The type of the fourth item</typeparam>
/// <typeparam name="T5">The type of the fifth item</typeparam>
public readonly ref struct RefTuple<T1, T2, T3, T4, T5>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5
)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where T3 : allows ref struct
    where T4 : allows ref struct
    where T5 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// The third item of the tuple.
    /// </summary>
    public T3 Item3 { get; } = item3;

    /// <summary>
    /// The fourth item of the tuple.
    /// </summary>
    public T4 Item4 { get; } = item4;

    /// <summary>
    /// The fifth item of the tuple.
    /// </summary>
    public T5 Item5 { get; } = item5;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5)
    {
        item1 = Item1;
        item2 = Item2;
        item3 = Item3;
        item4 = Item4;
        item5 = Item5;
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <param name="item3">The third item of the tuple</param>
/// <param name="item4">The fourth item of the tuple</param>
/// <param name="item5">The fifth item of the tuple</param>
/// <param name="item6">The sixth item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
/// <typeparam name="T3">The type of the third item</typeparam>
/// <typeparam name="T4">The type of the fourth item</typeparam>
/// <typeparam name="T5">The type of the fifth item</typeparam>
/// <typeparam name="T6">The type of the sixth item</typeparam>
public readonly ref struct RefTuple<T1, T2, T3, T4, T5, T6>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5,
    T6 item6
)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where T3 : allows ref struct
    where T4 : allows ref struct
    where T5 : allows ref struct
    where T6 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// The third item of the tuple.
    /// </summary>
    public T3 Item3 { get; } = item3;

    /// <summary>
    /// The fourth item of the tuple.
    /// </summary>
    public T4 Item4 { get; } = item4;

    /// <summary>
    /// The fifth item of the tuple.
    /// </summary>
    public T5 Item5 { get; } = item5;

    /// <summary>
    /// The sixth item of the tuple.
    /// </summary>
    public T6 Item6 { get; } = item6;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <param name="item6">The sixth item of the tuple</param>
    public void Deconstruct(
        out T1 item1,
        out T2 item2,
        out T3 item3,
        out T4 item4,
        out T5 item5,
        out T6 item6
    )
    {
        item1 = Item1;
        item2 = Item2;
        item3 = Item3;
        item4 = Item4;
        item5 = Item5;
        item6 = Item6;
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <param name="item3">The third item of the tuple</param>
/// <param name="item4">The fourth item of the tuple</param>
/// <param name="item5">The fifth item of the tuple</param>
/// <param name="item6">The sixth item of the tuple</param>
/// <param name="item7">The seventh item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
/// <typeparam name="T3">The type of the third item</typeparam>
/// <typeparam name="T4">The type of the fourth item</typeparam>
/// <typeparam name="T5">The type of the fifth item</typeparam>
/// <typeparam name="T6">The type of the sixth item</typeparam>
/// <typeparam name="T7">The type of the seventh item</typeparam>
public readonly ref struct RefTuple<T1, T2, T3, T4, T5, T6, T7>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5,
    T6 item6,
    T7 item7
)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where T3 : allows ref struct
    where T4 : allows ref struct
    where T5 : allows ref struct
    where T6 : allows ref struct
    where T7 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// The third item of the tuple.
    /// </summary>
    public T3 Item3 { get; } = item3;

    /// <summary>
    /// The fourth item of the tuple.
    /// </summary>
    public T4 Item4 { get; } = item4;

    /// <summary>
    /// The fifth item of the tuple.
    /// </summary>
    public T5 Item5 { get; } = item5;

    /// <summary>
    /// The sixth item of the tuple.
    /// </summary>
    public T6 Item6 { get; } = item6;

    /// <summary>
    /// The seventh item of the tuple.
    /// </summary>
    public T7 Item7 { get; } = item7;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <param name="item6">The sixth item of the tuple</param>
    /// <param name="item7">The seventh item of the tuple</param>
    public void Deconstruct(
        out T1 item1,
        out T2 item2,
        out T3 item3,
        out T4 item4,
        out T5 item5,
        out T6 item6,
        out T7 item7
    )
    {
        item1 = Item1;
        item2 = Item2;
        item3 = Item3;
        item4 = Item4;
        item5 = Item5;
        item6 = Item6;
        item7 = Item7;
    }
}

/// <summary>
/// A tuple-like struct that allows ref structs.
/// </summary>
/// <param name="item1">The first item of the tuple</param>
/// <param name="item2">The second item of the tuple</param>
/// <param name="item3">The third item of the tuple</param>
/// <param name="item4">The fourth item of the tuple</param>
/// <param name="item5">The fifth item of the tuple</param>
/// <param name="item6">The sixth item of the tuple</param>
/// <param name="item7">The seventh item of the tuple</param>
/// <param name="item8">The eighth item of the tuple</param>
/// <typeparam name="T1">The type of the first item</typeparam>
/// <typeparam name="T2">The type of the second item</typeparam>
/// <typeparam name="T3">The type of the third item</typeparam>
/// <typeparam name="T4">The type of the fourth item</typeparam>
/// <typeparam name="T5">The type of the fifth item</typeparam>
/// <typeparam name="T6">The type of the sixth item</typeparam>
/// <typeparam name="T7">The type of the seventh item</typeparam>
/// <typeparam name="T8">The type of the eighth item</typeparam>
public readonly ref struct RefTuple<T1, T2, T3, T4, T5, T6, T7, T8>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5,
    T6 item6,
    T7 item7,
    T8 item8
)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where T3 : allows ref struct
    where T4 : allows ref struct
    where T5 : allows ref struct
    where T6 : allows ref struct
    where T7 : allows ref struct
    where T8 : allows ref struct
{
    /// <summary>
    /// The first item of the tuple.
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item of the tuple.
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <summary>
    /// The third item of the tuple.
    /// </summary>
    public T3 Item3 { get; } = item3;

    /// <summary>
    /// The fourth item of the tuple.
    /// </summary>
    public T4 Item4 { get; } = item4;

    /// <summary>
    /// The fifth item of the tuple.
    /// </summary>
    public T5 Item5 { get; } = item5;

    /// <summary>
    /// The sixth item of the tuple.
    /// </summary>
    public T6 Item6 { get; } = item6;

    /// <summary>
    /// The seventh item of the tuple.
    /// </summary>
    public T7 Item7 { get; } = item7;

    /// <summary>
    /// The eighth item of the tuple.
    /// </summary>
    public T8 Item8 { get; } = item8;

    /// <summary>
    /// Deconstruct the tuple.
    /// </summary>
    /// <param name="item1">The first item of the tuple</param>
    /// <param name="item2">The second item of the tuple</param>
    /// <param name="item3">The third item of the tuple</param>
    /// <param name="item4">The fourth item of the tuple</param>
    /// <param name="item5">The fifth item of the tuple</param>
    /// <param name="item6">The sixth item of the tuple</param>
    /// <param name="item7">The seventh item of the tuple</param>
    /// <param name="item8">The eighth item of the tuple</param>
    public void Deconstruct(
        out T1 item1,
        out T2 item2,
        out T3 item3,
        out T4 item4,
        out T5 item5,
        out T6 item6,
        out T7 item7,
        out T8 item8
    )
    {
        item1 = Item1;
        item2 = Item2;
        item3 = Item3;
        item4 = Item4;
        item5 = Item5;
        item6 = Item6;
        item7 = Item7;
        item8 = Item8;
    }
}
