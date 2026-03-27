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
using System.Linq;

namespace ZParse.Util;

internal static class Friendly
{
    public static string Pluralize(string noun, int count)
    {
        ArgumentNullException.ThrowIfNull(noun);

        if (count == 1)
            return noun;

        return noun + "s";
    }

    public static string List(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        // Keep the order stable
        var unique = items.Distinct().ToList();

        return unique.Count switch
        {
            0 => throw new ArgumentException(
                "Friendly list formatting requires at least one element.",
                nameof(items)
            ),
            1 => unique.Single(),
            _ => $"{string.Join(", ", unique.Take(unique.Count - 1))} or {unique.Last()}",
        };
    }

    public static string Clip(string value, int maxLength)
    {
        if (value.Length > maxLength)
            return value[..(maxLength - 3)] + "...";
        return value;
    }
}
