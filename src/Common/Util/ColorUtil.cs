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

namespace Essentials.Common.Util {

    public static class ColorUtil {

        public static ConsoleColor UnityColorToConsoleColor(Color color) {
            if (color == Color.black)
                return ConsoleColor.Black;
            if (color == Color.green)
                return ConsoleColor.Green;
            if (color == Color.blue)
                return ConsoleColor.Blue;
            if (color == Color.cyan)
                return ConsoleColor.Cyan;
            if (color == Color.yellow)
                return ConsoleColor.Yellow;
            if (color == Color.gray)
                return ConsoleColor.Gray;
            if (color == Color.magenta)
                return ConsoleColor.Magenta;
            if (color == Color.red)
                return ConsoleColor.Red;
            return ConsoleColor.White;
        }

        public static bool HasColor(string message) {
            return message.Contains("<") && message.Contains(">") &&
                !string.IsNullOrEmpty(message.Split('<')[1].Split('>')[0]);
        }

        public static Color GetColorFromString(ref string message) {
            Color? color;

            if (!message.Contains("<") || !message.Contains(">")) {
                return Color.green;
            }

            var rawColor = message.Split('<')[1].Split('>')[0];

            if (string.IsNullOrEmpty(rawColor)) {
                return Color.green;
            }

            // Try get color from name
            switch (rawColor.Trim().ToLower()) {
                case "black":
                    color = Color.black;
                    break;
                case "blue":
                    color = Color.blue;
                    break;
                case "clear":
                    color = Color.clear;
                    break;
                case "cyan":
                    color = Color.cyan;
                    break;
                case "gray":
                    color = Color.gray;
                    break;
                case "green":
                    color = Color.green;
                    break;
                case "grey":
                    color = Color.grey;
                    break;
                case "magenta":
                    color = Color.magenta;
                    break;
                case "red":
                    color = Color.red;
                    break;
                case "white":
                    color = Color.white;
                    break;
                case "yellow":
                    color = Color.yellow;
                    break;
                default:
                    color = UnturnedChat.GetColorFromHex(rawColor);
                    break;
            }

            // Remove <color>
            if (color.HasValue) {
                message = message.Replace($"<{rawColor}>", "");
            }

            return color ?? Color.green;
        }

    }

}