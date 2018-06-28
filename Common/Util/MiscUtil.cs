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

using System.Collections.Generic;
using System.Text;

namespace Essentials.Common.Util
{
    public static class MiscUtil
    {
        public static string ValuesToString<T>(IEnumerable<T> enumarable, string separator = ", ",
            string start = "[", string end = "]")
        {
            var sb = new StringBuilder(start);
            var enumerator = enumarable.GetEnumerator();

            while (enumerator.MoveNext())
            {
                sb.Append(enumerator.Current?.ToString() ?? "null");
                sb.Append(separator);
            }

            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - separator.Length, separator.Length);
            }

            return sb.Append(end).ToString();
        }
    }
}