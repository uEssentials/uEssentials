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
using System.Collections.Generic;
using Essentials.Api;
using Essentials.Api.Event;
using Essentials.Core;
using Essentials.NativeModules.Kit.Commands;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace Essentials.NativeModules.Kit {

    internal class KitEventHandler {

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            var playerId = player.CSteamID.m_SteamID;
            var gCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;

            if (CommandKit.GlobalCooldown.ContainsKey(playerId) && EssCore.Instance.Config.Kit.ResetGlobalCooldownWhenDie) {
                CommandKit.GlobalCooldown[playerId] = DateTime.Now.AddSeconds(-gCooldown);
            }

            if (!CommandKit.Cooldowns.ContainsKey(playerId)) return;

            if (CommandKit.Cooldowns[playerId] == null) {
                CommandKit.Cooldowns.Remove(playerId);
                return;
            }

            var playerCooldowns = CommandKit.Cooldowns[playerId];
            var keys = new List<string>(playerCooldowns.Keys);
            var km = KitModule.Instance.KitManager;

            foreach (var kitName in keys) {
                var kit = km.GetByName(kitName);

                if (kit == null) {
                    playerCooldowns.Remove(kitName);
                    continue;
                }

                if (kit.ResetCooldownWhenDie) {
                    playerCooldowns[kitName] = DateTime.Now.AddSeconds(-kit.Cooldown);
                }
            }
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        void OnPlayerDisconnected(UnturnedPlayer player) {
            var playerId = player.CSteamID.m_SteamID;

            if (CommandKit.Cooldowns.Count == 0 ||
                !CommandKit.Cooldowns.ContainsKey(playerId)) {
                return;
            }

            if (CommandKit.Cooldowns[playerId] == null) {
                CommandKit.Cooldowns.Remove(playerId);
                return;
            }

            var playerCooldowns = CommandKit.Cooldowns[playerId];
            var keys = new List<string>(playerCooldowns.Keys);

            /*
                Remove from list if cooldown has expired.

                Global and per kit
            */
            var gCooldown = UEssentials.Config.Kit.GlobalCooldown;

            if (CommandKit.GlobalCooldown.ContainsKey(playerId) &&
                CommandKit.GlobalCooldown[playerId].AddSeconds(gCooldown) < DateTime.Now) {
                CommandKit.GlobalCooldown.Remove(playerId);
            }

            var kitManager = KitModule.Instance.KitManager;
            foreach (var kitName in keys) {
                var kit = kitManager.GetByName(kitName);

                if (kit == null || playerCooldowns[kitName].AddSeconds(kit.Cooldown) < DateTime.Now) {
                    playerCooldowns.Remove(kitName);
                }
            }

            if (playerCooldowns.Count == 0) {
                CommandKit.Cooldowns.Remove(playerId);
            }
        }

    }

}