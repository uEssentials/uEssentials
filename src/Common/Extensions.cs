#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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
            foreach (var obj in src) {
                action(obj);
            }
        }

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> src, Func<T, bool> predicate) {
            return src.Where(t => !predicate(t));
        }

        public static bool None<T>(this IEnumerable<T> src, Func<T, bool> predicate) {
            return !src.Any(predicate);
        }

    }

    public static class DictionaryExtensions {

        public static V GetOrDefault<K, V>(this Dictionary<K, V> src, K key, V def) {
            V val;
            return src.TryGetValue(key, out val) ? val : def;
        }

    }

/*
    Unused...

    public static class MiscExtensions {

        public static Predicate<T> And<T>(this Predicate<T> src, Predicate<T> other) {
            return p => src(p) && other(p);
        }

        public static Predicate<T> Or<T>(this Predicate<T> src, Predicate<T> other) {
            return p => src(p) || other(p);
        }

        public static T TryCast<T>(this object o, Action<T> successCallback) where T : class {
            var cast = o as T;
            if (cast != null) {
                successCallback?.Invoke(cast);
                return cast;
            }
            return null;
        }

    }
*/
}