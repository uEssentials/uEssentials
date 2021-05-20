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

using Essentials.Api;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using Rocket.Core;
using Rocket.Core.RCON;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Essentials.Core.Command {


    internal class ConsoleSource : ICommandSource
    {

        private static ConsoleSource _instance;
        private static readonly object LockObj = new object();

        public string Id => CSteamID.Nil.ToString();

        public string DisplayName => "*console*";

        public bool IsConsole => true;

        public bool IsAdmin => true;

        public List<string> Permissions => new List<string> { "*" };

        internal static ConsoleSource Instance {
            get {
                lock (LockObj) {
                    return _instance ?? (_instance = new ConsoleSource());
                }
            }
        }

        public bool HasPermission(string permission) => true;

        public void SendMessage(object message) {
            SendMessage(message, Color.green);
        }

        public void SendMessage(object message, Color color) {
            string sMessage = message is string
                ? AeiouToAscii((string) message)
                : message.ToString();

            try {
                if (R.Settings.Instance.RCON.Enabled) {
                    RCONServer.Broadcast(sMessage);
                }
            } catch (Exception ex) {
                UEssentials.Logger.LogError("Failed to broadcast a message to RCON.");
                UEssentials.Logger.LogException(ex);
            }

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ColorUtil.UnityColorToConsoleColor(color);
            Console.WriteLine(sMessage);
            Console.ForegroundColor = oldColor;
        }


        public override string ToString() {
            return DisplayName;
        }

        private static string AeiouToAscii(string str) {
            var chars = str.ToCharArray();

            for (var i = 0; i < chars.Length; i++) {
                if (chars[i] >= 224 && chars[i] <= 229) chars[i] = 'a';
                if (chars[i] >= 192 && chars[i] <= 197) chars[i] = 'A';

                if (chars[i] >= 232 && chars[i] <= 235) chars[i] = 'e';
                if (chars[i] >= 200 && chars[i] <= 203) chars[i] = 'E';

                if (chars[i] >= 236 && chars[i] <= 239) chars[i] = 'i';
                if (chars[i] >= 204 && chars[i] <= 207) chars[i] = 'I';

                if (chars[i] >= 242 && chars[i] <= 246) chars[i] = 'o';
                if (chars[i] >= 210 && chars[i] <= 214) chars[i] = 'O';

                if (chars[i] >= 249 && chars[i] <= 252) chars[i] = 'u';
                if (chars[i] >= 218 && chars[i] <= 220) chars[i] = 'U';

                if (chars[i] == 231) chars[i] = 'c';
                if (chars[i] == 199) chars[i] = 'C';
            }

            return new string(chars);
        }

        public void DispatchCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            if (command.StartsWith("/"))
                command = command.Substring(1);

            R.Commands.Execute(null, command);
        }
    }

}