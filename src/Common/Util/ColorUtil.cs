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
using Rocket.Unturned.Chat;
using UnityEngine;

namespace Essentials.Common.Util
{
    public static class ColorUtil
    {
        public static ConsoleColor UnityColorToConsoleColor( Color color )
        {
            if ( color == Color.black )
                return ConsoleColor.Black;
            if ( color == Color.green )
                return ConsoleColor.Green;
            if ( color == Color.blue )
                return ConsoleColor.Blue;
            if ( color == Color.cyan )
                return ConsoleColor.Cyan;
            if ( color == Color.yellow )
                return ConsoleColor.Yellow;
            if ( color == Color.gray )
                return ConsoleColor.Gray;
            if ( color == Color.magenta )
                return ConsoleColor.Magenta;
            if ( color == Color.red )
                return ConsoleColor.Red;
            return ConsoleColor.White;
        }

        public static Color GetMessageColor( ref string message )
        {
            var color = Color.green;

            if ( message.IndexOfAny( new[] {'<', '>'} ) == -1 ) return color;

            var rawColor = message.Split( '<' )[1].Split( '>' )[0];
            color = UnturnedChat.GetColorFromName( rawColor, color );// try parse color
            message = message.Replace( $"<{rawColor}>", "" );// remove color from text

            return color;
        }
    }
}
