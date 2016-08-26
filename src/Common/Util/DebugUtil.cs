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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Essentials.Common.Util {

    public static class DebugUtil {

        public static void DebugObject(object obj, string objName = null) {
            Console.Write("[DEBUG] ");
            if (objName != null) Console.Write(objName + " = ");
            Console.Write(ObjectToString(obj));
            Console.WriteLine();
            Console.WriteLine();
        }

        public static string ObjectToString(object obj) {
            if (obj == null)
                return "Null";

            var sb = new StringBuilder();

            if (obj is IEnumerable && !(obj is string)) {
                var en = (IEnumerable) obj;
                var enumerable = en as IList<object> ?? en.Cast<object>().ToList();

                if (enumerable.Count == 0) {
                    sb.Append("Empty");
                } else {
                    sb.Append("[");
                    foreach (var val in enumerable) {
                        sb.Append(val);
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append("]");
                }
            } else {
                sb.Append(obj);
            }

            sb.Append(" (");
            sb.Append(obj.GetType());
            sb.Append(") ");

            return sb.ToString();
        }

        public static string ToBinaryString(long val, long numBits = 16) {
            var bit = 0;
            var buf = new char[numBits];
            while (numBits > 0) {
                buf[--numBits] = (val & (1 << bit++)) > 0 ? '1' : '0';
            }
            return new string(buf);
        }
    }

}