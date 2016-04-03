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

using System.Text;

namespace Essentials.Common.Util
{
    public static class MiscUtil
    {
        public static string ArrayToString<T>( T[] array, string separator = ", ",
                                               string start = "[", string end = "]"  )
        {
            var sb = new StringBuilder( start );
            var arrLength = array.Length;
            
            for (int i = 0; i < arrLength; i++)
            {
                sb.Append( array[i] );
                
                if ( (i + 1) != arrLength )
                {
                    sb.Append( separator );
                }
            }
            
            sb.Append( end );
            
            return sb.ToString();
        }
    }
}