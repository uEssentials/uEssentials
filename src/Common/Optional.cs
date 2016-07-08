/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
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

using System;

namespace Essentials.Common {

    /// <summary>
    ///  A container object which may or may not contain a non-null value.
    ///  If a value is present, <see cref="IsPresent"/> will return <code>true</code> and
    /// <see cref="Value"/> will return the value.
    /// </summary>
    public class Optional<T> {

        private static readonly Optional<T> EMPTY = new Optional<T>(default(T));
        private readonly T _value;

        public bool IsPresent => _value != null;

        public bool IsAbsent => _value == null;

        public T Value {
            get {
                if (_value == null) {
                    throw new InvalidOperationException("No value present.");
                }

                return _value;
            }
        }

        private Optional(T value) {
            _value = value;
        }

        public void IfPresent(Action<T> consumer) {
            Preconditions.NotNull(consumer, "consumer cannot be null");

            if (IsPresent) {
                consumer(Value);
            }
        }

        public void IfAbsent(Action<T> consumer) {
            Preconditions.NotNull(consumer, "consumer cannot be null");

            if (IsAbsent) {
                consumer(Value);
            }
        }

        public T OrElse(T value) {
            return IsPresent ? Value : value;
        }

        public static Optional<T> Of(T value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value), "value cannot be null");
            }

            return new Optional<T>(value);
        }

        public static Optional<T> OfNullable(T value) {
            return new Optional<T>(value);
        }

        public static Optional<T> Empty() {
            return EMPTY;
        }

        public override string ToString() {
            return IsPresent ? $"Optional[{Value}]" : "Optional.Empty";
        }

    }

}