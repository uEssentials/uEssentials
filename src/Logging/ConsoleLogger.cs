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

namespace Essentials.Logging {

    public class ConsoleLogger {

        private string Prefix { get; }

        public ConsoleLogger(string prefix) {
            Prefix = prefix;
        }

        public void LogError(string message, bool parseColors) {
            Log(message, ConsoleColor.Red, Prefix + "[ERROR] ", parseColors: parseColors);
        }

        public void LogWarning(string message, bool parseColors) {
            Log(message, ConsoleColor.Yellow, Prefix + "[WARN] ", parseColors: parseColors);
        }

        public void LogInfo(string message, bool parseColors) {
            Log(message, ConsoleColor.Green, Prefix + "[INFO] ", parseColors: parseColors);
        }

        public void LogDebug(string message, bool parseColors) {
            Log(message, ConsoleColor.DarkGray, Prefix + "[DEBUG] ", parseColors: parseColors);
        }

        public void LogError(string message) => LogError(message, false);

        public void LogWarning(string message) => LogWarning(message, false);

        public void LogInfo(string message) => LogInfo(message, false);

        public void LogDebug(string message) => LogDebug(message, false);

        public void Log(string message, ConsoleColor color, string prefix = "default", 
                        string suffix = "default", bool parseColors = false) {
            if (prefix == "default") {
                prefix = Prefix;
            }
            if (suffix == "default") {
                suffix = Environment.NewLine;
            }
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            lock (Console.Out) {
                Write(prefix + message + suffix, parseColors);
            }
            Console.ForegroundColor = lastColor;
        }

        private void Write(string text, bool parseColors = false) {
            if (!parseColors) {
                Console.Write(text);
                return;
            }
            var colorBuf = new char[11];
            var lastClrIdx = 0;

            for (var i = 0; i < text.Length; i++) {
                var c = text[i];
                if (c == '~') {
                    Console.Write(text.Substring(lastClrIdx, i - lastClrIdx));
                    var j = 0;
                    for (;;) {
                        c = text[++i];
                        if (c == '~' || i == text.Length || j == 11) break;
                        if (c >= 'A' && c <= 'Z') {
                            c = (char) (c + 32); // To lower case
                        }
                        colorBuf[j++] = c;
                    }
                    i++; // Skip ~
                    lastClrIdx = i;
                    var strClr = new string(colorBuf, 0, j);
                    switch (strClr) {
                        case "black":
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        case "darkblue":
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            break;
                        case "darkgreen":
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case "darkcyan":
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            break;
                        case "darkred":
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            break;
                        case "darkmagenta":
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            break;
                        case "darkyellow":
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                        case "gray":
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case "darkgray":
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case "blue":
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case "green":
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case "cyan":
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case "red":
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case "magenta":
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            break;
                        case "yellow":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case "white":
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                    }
                }
                if (i + 1 == text.Length && lastClrIdx != text.Length) {
                    Console.Write(text.Substring(lastClrIdx, (i - lastClrIdx) + 1));
                }
            }
        }

    }

}