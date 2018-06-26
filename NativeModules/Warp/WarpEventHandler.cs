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
using Essentials.Api.Event;
using Essentials.Api.Unturned;
using Essentials.I18n;
using Essentials.NativeModules.Warp.Commands;
using Rocket.Unturned.Player;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;

namespace Essentials.NativeModules.Warp {

    class WarpEventHandler {

        [SubscribeEvent(EventType.PLAYER_UPDATE_POSITION)]
        void OnPlayerMove(UnturnedPlayer player, Vector3 newPosition) {
            if (!UEssentials.Config.Warp.CancelTeleportWhenMove || !CommandWarp.Delay.ContainsKey(player.CSteamID.m_SteamID)) {
                return;
            }

            CommandWarp.Delay[player.CSteamID.m_SteamID].Cancel();
            CommandWarp.Delay.Remove(player.CSteamID.m_SteamID);

            UPlayer.TryGet(player, p => {
                EssLang.Send(p, "TELEPORT_CANCELLED_MOVED");
            });
        }

    }

}