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

namespace Essentials.Logging {

    public class ConsoleLogger {

        private string Prefix { get; }

        public ConsoleLogger(string prefix) {
            Prefix = prefix;
        }

        public void LogError(string message) {
            Log(message, ConsoleColor.Red, Prefix + "[ERROR] ");
        }

        public void LogWarning(string message) {
            Log(message, ConsoleColor.Yellow, Prefix + "[WARN] ");
        }

        public void LogInfo(string message) {
            Log(message, ConsoleColor.Green, Prefix + "[INFO] ");
        }

        public void LogDebug(string message) {
            Log(message, ConsoleColor.DarkGray, Prefix + "[DEBUG] ");
        }

        public void Log(string message, ConsoleColor color, string prefix = "default", string suffix = "\n") {
            if (prefix.Equals("default"))
                prefix = Prefix;

            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(prefix + message + suffix);
            Console.ForegroundColor = lastColor;
        }

    }

}