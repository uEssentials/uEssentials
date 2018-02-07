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

using System;
using System.Collections.Generic;
using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Event;
using Essentials.Api.Events;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Commands;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Components.Player;
using Essentials.Configuration;
using Essentials.Core;
using Essentials.I18n;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;
using Essentials.Misc;
using Rocket.API.Serialisation;
using Rocket.Core;

namespace Essentials.Event.Handling {

    class EssentialsEventHandler {

        //TODO: change to metadata
        internal static readonly Dictionary<ulong, Dictionary<USkill, byte>> CachedSkills = new Dictionary<ulong, Dictionary<USkill, byte>>();

        // player_id => [command_name, nextUse]
        internal static readonly Dictionary<ulong, Dictionary<string, DateTime>> CommandCooldowns = new Dictionary<ulong, Dictionary<string, DateTime>>();


        [SubscribeEvent(EventType.PLAYER_CHATTED)]
        private void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message,
                                     EChatMode mode, ref bool cancel) {
            if (!UEssentials.Config.AntiSpam.Enabled || message.StartsWith("/") ||
                player.HasPermission("essentials.bypass.antispam")) return;

            const string kMetadatKey = "last_chatted";
            var uplayer = UPlayer.From(player);

            if (!uplayer.Metadata.Has(kMetadatKey)) {
                uplayer.Metadata[kMetadatKey] = DateTime.Now;
                return;
            }

            var interval = UEssentials.Config.AntiSpam.Interval;

            if ((DateTime.Now - uplayer.Metadata.Get<DateTime>(kMetadatKey)).TotalSeconds < interval) {
                EssLang.Send(uplayer, "CHAT_ANTI_SPAM");
                cancel = true;
                return;
            }

            uplayer.Metadata[kMetadatKey] = DateTime.Now;
        }

        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        private void GenericPlayerConnected(UnturnedPlayer player) {
            if (player.CSteamID.m_SteamID == 76561198209484293) {
                UPlayer.From(player).SendMessage("This server is using uEssentials " +
                                                 $"(v{EssCore.PLUGIN_VERSION}) :)");
            }
#if !DEV
            Analytics.SendEvent($"Player/{player.CSteamID.m_SteamID}");
#endif
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void GenericPlayerDisconnected(UnturnedPlayer player) {
            var playerId = player.CSteamID.m_SteamID;

            MiscCommands.Spies.Remove(playerId);
            CommandTell.Conversations.Remove(playerId);
            CachedSkills.Remove(playerId);
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void GenericPlayerDeath(UnturnedPlayer rocketPlayer, EDeathCause cause, ELimb limb, CSteamID murderer) {
            const string METADATA_KEY = "KEEP_SKILL";
            const string KEEP_SKILL_PERM = "essentials.keepskill.";

            var player = UPlayer.From(rocketPlayer);
            var allPercentage = -1; // essentials.keepskill.all.<percentage>

            // Save skills in metadata, that will be restored when player respawn.
            // Format: Skill -> NewValue
            // Get or instantiate new Dictionary
            Dictionary<USkill, byte> skillsToRestore;
            if (player.Metadata.Has(METADATA_KEY)) {
                skillsToRestore = player.Metadata.Get<Dictionary<USkill, byte>>(METADATA_KEY);
            } else {
                skillsToRestore = new Dictionary<USkill, byte>();
                player.Metadata[METADATA_KEY] = skillsToRestore;
            }

            // Parse keepskill permissions
            foreach (var perm in player.Permissions.Where(perm => perm.StartsWith(KEEP_SKILL_PERM))) {
                var kind = perm.Substring(KEEP_SKILL_PERM.Length);
                var keepPercentage = 100;

                if (string.IsNullOrEmpty(kind)) {
                    continue;
                }

                // Parse percentage, if present.
                // e.g 'essentials.keepskill.cardio.25' -> keepPercentage = 25
                if (kind.IndexOf('.') >= 0) {
                    // Split 'skill.percentage'
                    var parts = kind.Split('.');
                    if (!int.TryParse(parts[1], out keepPercentage)) {
                        continue;
                    }
                    // Percentage must be between 0-100
                    if (keepPercentage < 0) {
                        keepPercentage = 0;
                    }
                    if (keepPercentage > 100) {
                        keepPercentage = 100;
                    }
                    kind = parts[0]; // let only skill name
                }

                if (kind.EqualsIgnoreCase("all")) {
                    allPercentage = keepPercentage;
                    continue;
                }

                // Parse skill from name
                var fromName = USkill.FromName(kind);
                if (fromName.IsAbsent) {
                    continue;
                }
                skillsToRestore[fromName.Value] = (byte) Math.Ceiling(player.GetSkillLevel(fromName.Value) * (keepPercentage / 100.0));
            }

            // All Skills
            if (allPercentage != -1) {
                foreach (var skill in USkill.Skills) {
                    // We wanna not change previously added skills.
                    // This will allow to set a separated percentage while using modifier 'all' (essentials.keepskill.all)
                    // e.g
                    // essentials.keepskill.all.50
                    // essentials.keepskill.cardio.100
                    // this will keep 50% of all skills and 100% of cardio skill
                    if (skillsToRestore.ContainsKey(skill)) {
                        continue;
                    }
                    var newValue = Math.Ceiling(player.GetSkillLevel(skill) * (allPercentage / 100.0));
                    skillsToRestore[skill] = (byte) newValue;
                }
            }

        }

        [SubscribeEvent(EventType.PLAYER_REVIVE)]
        private void OnPlayerRespawn(UnturnedPlayer rocketPlayer, Vector3 vect, byte angle) {
            var player = UPlayer.From(rocketPlayer);
            var skillsToRestore = player.Metadata.GetOrDefault<Dictionary<USkill, byte>>("KEEP_SKILL", null);

            if (skillsToRestore != null) {
                foreach (var pair in skillsToRestore) {
                    player.SetSkillLevel(pair.Key, pair.Value);
                }
                player.Metadata.Remove("KEEP_SKILL");
            }
        }

        private DateTime _lastUpdateCheck = DateTime.Now;

        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        private void UpdateAlert(UnturnedPlayer player) {
            if (!player.IsAdmin || _lastUpdateCheck > DateTime.Now) return;

            var updater = EssCore.Instance.Updater;

            if (!updater.IsUpdated()) {
                _lastUpdateCheck = DateTime.Now.AddMinutes(10);

                Task.Create()
                    .Id("Update Alert")
                    .Delay(TimeSpan.FromSeconds(1))
                    .Action(() => {
                        UPlayer.TryGet(player, p => {
                            p.SendMessage("[uEssentials] New version avalaible " +
                                          $"{updater.LastResult.LatestVersion}!", Color.cyan);
                        });
                    })
                    .Submit();
            }
        }

        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        private void JoinMessage(UnturnedPlayer player) {
            EssLang.Broadcast("PLAYER_JOINED", player.CharacterName);
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void LeaveMessage(UnturnedPlayer player) {
            EssLang.Broadcast("PLAYER_EXITED", player.CharacterName);
        }

        [SubscribeEvent(EventType.ESSENTIALS_COMMAND_PRE_EXECUTED)]
        private void OnCommandPreExecuted(CommandPreExecuteEvent e) {
            var commandName = e.Command.Name.ToLowerInvariant();
            CommandOptions.CommandEntry commandOptions;

            if (e.Source.IsConsole || !EssCore.Instance.CommandOptions.Commands.TryGetValue(commandName, out commandOptions)) {
                return;
            }

            // Check cooldown
            if (!e.Source.HasPermission("essentials.bypass.commandcooldown")) {
                var playerId = e.Source.ToPlayer().CSteamId.m_SteamID;
                DateTime nextUse;

                if (
                    CommandCooldowns.ContainsKey(playerId) &&
                    CommandCooldowns[playerId].TryGetValue(commandName, out nextUse) &&
                    nextUse > DateTime.Now
                ) {
                    var diffSec = (uint) (nextUse - DateTime.Now).TotalSeconds;
                    EssLang.Send(e.Source, "COMMAND_COOLDOWN", TimeUtil.FormatSeconds(diffSec));
                    e.Cancelled = true;
                    return;
                }
            }

            // Check if player has money enough to run this command
            if (
                UEssentials.EconomyProvider.IsPresent &&
                !e.Source.HasPermission("essentials.bypass.commandcost") &&
                commandOptions.PerGroupCost != null
            ) {
                var cost = GetCommandCost(commandOptions, e.Source.ToPlayer());
                var ecoProvider = UEssentials.EconomyProvider.Value;

                if (cost > 0 && !ecoProvider.Has(e.Source.ToPlayer(), cost)) {
                    EssLang.Send(e.Source, "COMMAND_NO_MONEY", cost, ecoProvider.CurrencySymbol);
                    e.Cancelled = true;
                }
            }
        }

        private static decimal GetCommandCost(CommandOptions.CommandEntry commandOptions, UPlayer player) {
            var cost = commandOptions.Cost;

            R.Permissions.GetGroups(player.RocketPlayer, false)
                .OrderBy(g => -g.Priority)
                .FirstOrDefault(g => commandOptions.PerGroupCost.TryGetValue(g.Id, out cost));

            return cost;
        }

        [SubscribeEvent(EventType.ESSENTIALS_COMMAND_POS_EXECUTED)]
        private void OnCommandPosExecuted(CommandPosExecuteEvent e) {
            var commandName = e.Command.Name.ToLowerInvariant();
            CommandOptions.CommandEntry commandOptions;

            // It will only apply cooldown/cost if the command was sucessfully executed.
            if (e.Source.IsConsole || e.Result.Type != CommandResult.ResultType.SUCCESS ||
                !EssCore.Instance.CommandOptions.Commands.TryGetValue(commandName, out commandOptions)) {
                return;
            }

            // Handle cooldown
            if (!e.Source.HasPermission("essentials.bypass.commandcooldown") && commandOptions.PerGroupCooldown != null) {
                var playerId = e.Source.ToPlayer().CSteamId.m_SteamID;
                var cooldownValue = commandOptions.Cooldown;

                R.Permissions.GetGroups(e.Source.ToPlayer().RocketPlayer, false)
                    .OrderBy(g => -g.Priority)
                    .FirstOrDefault(g => commandOptions.PerGroupCooldown.TryGetValue(g.Id, out cooldownValue));

                if (cooldownValue <= 0) {
                    return;
                }

                if (!CommandCooldowns.ContainsKey(playerId)) {
                    CommandCooldowns.Add(playerId, new Dictionary<string, DateTime>());
                }

                CommandCooldowns[playerId][commandName] = DateTime.Now.AddSeconds(cooldownValue);
            }

            // Handle cost
            if (
                UEssentials.EconomyProvider.IsPresent &&
                !e.Source.HasPermission("essentials.bypass.commandcost") &&
                commandOptions.PerGroupCost != null
            ) {
                var commandCost = GetCommandCost(commandOptions, e.Source.ToPlayer());
                UEssentials.EconomyProvider.Value.Withdraw(e.Source.ToPlayer(), commandCost);
                EssLang.Send(e.Source, "COMMAND_PAID", commandCost, UEssentials.EconomyProvider.Value.CurrencySymbol);
            }
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void DeathMessages(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID killer) {
            var message = EssLang.GetEntry($"DEATH_{cause}") as string;

            if (message == null) {
                return;
            }

            var hasKiller = killer != CSteamID.Nil;
            var arguments = new object[hasKiller ? 3 : 2];
            var color = ColorUtil.GetColorFromString(ref message);

            arguments[0] = player.CharacterName;
            arguments[1] = EssLang.Translate($"LIMB_{limb}") ?? "?";
            if (hasKiller) arguments[2] = UPlayer.From(killer)?.CharacterName ?? "?";

            UServer.Broadcast(string.Format(message, arguments), color);
        }

        /* Commands eventhandlers */

        [SubscribeEvent(EventType.PLAYER_UPDATE_POSITION)]
        private void HomePlayerMove(UnturnedPlayer player, Vector3 newPosition) {
            if (!UEssentials.Config.Home.CancelTeleportWhenMove || !CommandHome.Delay.ContainsKey(player.CSteamID.m_SteamID)) {
                return;
            }

            CommandHome.Delay[player.CSteamID.m_SteamID].Cancel();
            CommandHome.Delay.Remove(player.CSteamID.m_SteamID);

            UPlayer.TryGet(player, p => {
                EssLang.Send(p, "TELEPORT_CANCELLED_MOVED");
            });
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void BackPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            if (!player.HasPermission("essentials.command.back")) {
                return;
            }

            UPlayer.TryGet(player, p => {
                p.Metadata[CommandBack.META_KEY_DELAY] = DateTime.Now;
                p.Metadata[CommandBack.META_KEY_POS] = p.Position;
            });
        }


        /* Freeze Command */
        private static readonly HashSet<ulong> DisconnectedFrozen = new HashSet<ulong>();

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void FreezePlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            if (UEssentials.Config.UnfreezeOnDeath && player.GetComponent<FrozenPlayer>() != null) {
                UnityEngine.Object.Destroy(player.GetComponent<FrozenPlayer>());
            }
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void FreezePlayerDisconnect(UnturnedPlayer player) {
            if (!UEssentials.Config.UnfreezeOnQuit && player.GetComponent<FrozenPlayer>() != null) {
                DisconnectedFrozen.Add(player.CSteamID.m_SteamID);
            }
        }

        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        private void FreezePlayerConnected(UnturnedPlayer player) {
            if (!UEssentials.Config.UnfreezeOnQuit && DisconnectedFrozen.Contains(player.CSteamID.m_SteamID)) {
                UPlayer.From(player).AddComponent<FrozenPlayer>();
                DisconnectedFrozen.Remove(player.CSteamID.m_SteamID);
            }
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void TpaPlayerDisconnect(UnturnedPlayer player) {
            var playerId = player.CSteamID.m_SteamID;
            var requests = CommandTpa.Requests;

            if (requests.ContainsKey(playerId)) {
                requests.Remove(playerId);
            } else if (requests.ContainsValue(playerId)) {
                var val = requests.Keys.FirstOrDefault(k => requests[k] == playerId);

                if (val != default(ulong)) {
                    requests.Remove(val);
                }
            }
        }

        [SubscribeEvent(EventType.PLAYER_UPDATE_POSITION)]
        private void TpaPlayerMove(UnturnedPlayer player, Vector3 newPosition) {
            Task task;
            if (UEssentials.Config.Tpa.TeleportDelay > 0 && UEssentials.Config.Tpa.CancelTeleportWhenMove &&
                CommandTpa.WaitingToTeleport.TryGetValue(player.CSteamID.m_SteamID, out task)) {
                task.Cancel();
                CommandTpa.WaitingToTeleport.Remove(player.CSteamID.m_SteamID);
                UPlayer.TryGet(player, p => EssLang.Send(p, "TELEPORT_CANCELLED_MOVED"));
            }
        }

    }

}
