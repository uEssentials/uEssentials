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
using System.IO;
using Essentials.Api;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.NativeModules.Kit.Commands;

namespace Essentials.NativeModules.Kit.Data {

    public class CooldownData {

        public string FilePath => UEssentials.DataFolder + Path.DirectorySeparatorChar + "kit_cooldowns.json";

        public void Load() {
            if (!File.Exists(FilePath)) {
                return;
            }

            var saved = JsonUtil.DeserializeFile<Dictionary<ulong, PlayerCooldown>>(FilePath);
            saved.ForEach(kv => {
                CommandKit.Cooldowns.Clear();
                CommandKit.GlobalCooldown.Clear();
                if (kv.Value.Kits != null) {
                    CommandKit.Cooldowns.Add(kv.Key, kv.Value.Kits);
                }
                if (!kv.Value.Global.Equals(default(DateTime))) {
                    CommandKit.GlobalCooldown.Add(kv.Key, kv.Value.Global);
                }
                ClearCooldowns(kv.Key);
            });
        }

        public void Save() {
            var toSave = new Dictionary<ulong, PlayerCooldown>();
            var playerIds = new HashSet<ulong>();
            var kitCooldowns = CommandKit.Cooldowns;
            var globalCooldowns = CommandKit.GlobalCooldown;

            kitCooldowns.ForEach(k => playerIds.Add(k.Key));
            globalCooldowns.ForEach(k => playerIds.Add(k.Key));

            playerIds.ForEach(id => {
                ClearCooldowns(id);

                var pCooldown = new PlayerCooldown();
                kitCooldowns.TryGetValue(id, out pCooldown.Kits);
                globalCooldowns.TryGetValue(id, out pCooldown.Global);

                toSave.Add(id, pCooldown);
            });

            JsonUtil.Serialize(FilePath, toSave);
        }

        /// <summary>
        /// Remove 'expired' cooldowns.
        /// </summary>
        /// <param name="playerId"></param>
        private void ClearCooldowns(ulong playerId) {
            var km = KitModule.Instance.KitManager;
            var gCooldown = UEssentials.Config.Kit.GlobalCooldown;

            if (CommandKit.GlobalCooldown.ContainsKey(playerId) &&
                CommandKit.GlobalCooldown[playerId].AddSeconds(gCooldown) < DateTime.Now) {
                CommandKit.GlobalCooldown.Remove(playerId);
            }

            if (!CommandKit.Cooldowns.ContainsKey(playerId)) {
                return;
            }

            if (CommandKit.Cooldowns[playerId] == null) {
                CommandKit.Cooldowns.Remove(playerId);
                return;
            }

            var playerCooldowns = CommandKit.Cooldowns[playerId];
            var keys = new List<string>(playerCooldowns.Keys);

            foreach (var kitName in keys) {
                var kit = km.GetByName(kitName);

                if (kit == null || playerCooldowns[kitName].AddSeconds(kit.Cooldown) < DateTime.Now) {
                    playerCooldowns.Remove(kitName);
                }
            }

            if (playerCooldowns.Count == 0) {
                CommandKit.Cooldowns.Remove(playerId);
            }
        }

    }

    internal struct PlayerCooldown {
        public Dictionary<string, DateTime> Kits;
        public DateTime Global;
    }

}