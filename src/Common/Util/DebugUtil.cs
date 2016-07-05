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
using System.Text;
using System.Collections;

namespace Essentials.Common.Util
{
    public static class DebugUtil
    {
        public static void DebugObject( object obj, string objName = null )
        {
            Console.Write( "[DEBUG] " );
            if ( objName != null ) Console.Write( objName + " = " );
            Console.Write( ObjectToString( obj ) );
            Console.WriteLine();
            Console.WriteLine();
        }

        public static string ObjectToString( object obj )
        {
            if (obj == null)
                return "Null";

            var sb = new StringBuilder();

            if ( obj is IEnumerable && !(obj is string) )
            {
                var en = obj as IEnumerable;

                if ( !en.GetEnumerator().MoveNext() )
                    sb.Append( "Empty" );
                else
                    AppendCollection( sb, en );
            }
            else
            {
                sb.Append( obj.ToString() );
            }

            sb.Append( " (" );
            sb.Append( obj.GetType() );
            sb.Append( ") " );

            return sb.ToString();
        }

        private static void AppendCollection( StringBuilder sb, IEnumerable array )
        {
            sb.Append( "[" );
            foreach ( var val in array )
            {
                sb.Append( val );
                sb.Append( ", " );
            }
            sb.Remove( sb.Length - 2, 2 );
            sb.Append( "]" );
        }
    }
}