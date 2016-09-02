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
using System.Globalization;
using UnityEngine;

namespace Essentials.Common.Util {

    public static class ColorUtil {

        public static ConsoleColor UnityColorToConsoleColor(Color color) {
            if (color == Color.black) return ConsoleColor.Black;
            if (color == Color.green) return ConsoleColor.Green;
            if (color == Color.blue) return ConsoleColor.Blue;
            if (color == Color.cyan) return ConsoleColor.Cyan;
            if (color == Color.yellow) return ConsoleColor.Yellow;
            if (color == Color.gray) return ConsoleColor.Gray;
            if (color == Color.magenta) return ConsoleColor.Magenta;
            if (color == Color.red) return ConsoleColor.Red;
            return ConsoleColor.White;
        }

        public static bool HasColor(string message) {
            int l, r;
            if ((l = message.IndexOf('<')) == -1 || (r = message.IndexOf('>')) == -1) {
                return false;
            }
            return message.Substring(++l, --r).Length > 0;
        }

        public static Color GetColorFromString(ref string message) {
            Color? color;
            int l, r;
            
            if ((l = message.IndexOf('<')) == -1 || (r = message.IndexOf('>')) == -1) {
                return Color.green;
            }

            var rawColor = message.Substring(++l, --r);

            if (string.IsNullOrEmpty(rawColor)) {
                return Color.green;
            }

            // Try get color from name
            switch (rawColor.Trim().ToLower()) {
                case "black": color = Color.black; break;
                case "blue": color = Color.blue; break;
                case "clear": color = Color.clear; break;
                case "cyan": color = Color.cyan; break;
                case "gray": color = Color.gray; break;
                case "green": color = Color.green; break;
                case "grey": color = Color.grey; break;
                case "magenta": color = Color.magenta; break;
                case "red": color = Color.red; break;
                case "white": color = Color.white; break;
                case "yellow": color = Color.yellow; break;
                default: color = ColorFromHex(rawColor); break;
            }

            // Remove <color>
            if (color.HasValue) {
                message = message.Replace($"<{rawColor}>", string.Empty);
            }

            return color ?? Color.green;
        }

        private static Color? ColorFromHex(string rawColor) {
            var len = rawColor.Length;
            int hex;

            if (len == 0) {
                return null;
            }

            if (rawColor[0] == '#') {
                rawColor = rawColor.Substring(1);
            }

            // <RGB> to <RRGGBB>
            if (len == 3) {
                var chars = new char[6];
                chars[0] = rawColor[0];
                chars[1] = rawColor[0];
                chars[2] = rawColor[1];
                chars[3] = rawColor[1];
                chars[4] = rawColor[2];
                chars[5] = rawColor[2];
                rawColor = new string(chars);
            }

            if (!int.TryParse(rawColor, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hex)) {
                System.Diagnostics.Debug.Print("Failed to parse hex color '{0}'.", rawColor);
                return null;
            }
            
            return new Color(
                ((hex >> 16) & 0xFF) / 255F,
                ((hex >> 8) & 0xFF) / 255F, 
                ((hex) & 0xFF) / 255F
            );
        }
    }

}
