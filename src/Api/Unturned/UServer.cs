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
using Essentials.Core;
using Essentials.Core.Command;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Api.Unturned {

    public class UServer {

        public static IEnumerable<UPlayer> Players => EssCore.Instance.ConnectedPlayers.Values;

        public static byte MaxPlayers {
            get { return Provider.maxPlayers; }
            set { Provider.maxPlayers = value; }
        }

        public static string ServerName {
            get { return Provider.serverName; }
            set { Provider.serverName = value; }
        }

        public static void Broadcast(object message) {
            Broadcast(message, Color.yellow);
        }

        public static void Broadcast(object message, Color color) {
            UnturnedChat.Say(message?.ToString() ?? "null", color);
        }

        public static void DispatchCommand(string command) {
            ConsoleSource.Instance.DispatchCommand(command);
        }

    }

}