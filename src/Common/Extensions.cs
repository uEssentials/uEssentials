#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Essentials.Common {

    public static class StringExtensions {

        public static bool EqualsIgnoreCase(this string str1, string str2) {
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool ContainsIgnoreCase(this string str, string part) {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, part, CompareOptions.IgnoreCase) >= 0;
        }

        public static bool IsNullOrEmpty(this string str) {
            return string.IsNullOrEmpty(str);
        }

        public static string Capitalize(this string str) {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLowerInvariant());
        }

    }

    public static class EnumerableExtensions {

        public static void ForEach<T>(this IEnumerable<T> src, Action<T> action) {
            foreach (var obj in src.ToList()) 
            {
                action(obj);
            }
        }

        public static bool None<T>(this IEnumerable<T> src, Func<T, bool> predicate) {
            return !src.Any(predicate);
        }

        public static bool DeepEquals<T>(this List<T> src, List<T> dest) {
            if (src.Count != dest.Count) {
                return false;
            }
            return !src.Where((t, i) => !t.Equals(dest[i])).Any();
        }
    }

}