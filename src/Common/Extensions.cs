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
using System.Collections.Generic;
using System.Globalization;
using Essentials.Common.Util;

namespace Essentials.Common
{
    public static class Extensions
    {
        public static bool EqualsIgnoreCase( this string str1, string str2 )
        {
            return string.Compare( str1, str2, StringComparison.OrdinalIgnoreCase ) == 0;
        }

        public static bool ContainsIgnoreCase( this string str, string part )
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf( str, part, CompareOptions.IgnoreCase ) >= 0;
        }
        
        public static bool IsNullOrEmpty( this string str )
        {
            return string.IsNullOrEmpty( str );
        }

        public static string Capitalize( this string str )
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase( str.ToLowerInvariant() );
        }
        
        public static string Format( this string str, params object[] args ) 
        {
            return string.Format( str, args );   
        }


        public static void ForEach<T>( this T[] array, Action<T> act )
        {
            foreach ( var obj in array )
            {
                act( obj );
            }   
        }

        public static void ForEach<T>( this IEnumerable<T> enume , Action<T> act )
        {
            foreach ( var obj in enume )
            {
                act( obj );
            }   
        }
        
        public static string ArrayToString<T>( this T[] arr )
        {
            return MiscUtil.ArrayToString( arr );
        }
    }
}