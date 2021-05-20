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
using Essentials.Misc;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;

namespace Essentials.Event.Handling {

    class EssentialsEventHandler {

        // player_id => [command_name, nextUse]
        internal static readonly Dictionary<ulong, Dictionary<string, DateTime>> CommandCooldowns = new Dictionary<ulong, Dictionary<string, DateTime>>();

        [SubscribeEvent(EventType.PLAYER_CHATTED)]
        private void OnPlayerChatted(UnturnedPlayer player, ref Color c, string message,
                                     EChatMode m, ref bool cancel) {
            if (
                !UEssentials.Config.AntiSpam.Enabled ||
                message.StartsWith("/") ||
                player.HasPermission("essentials.bypass.antispam")
            ) return;

            const string METADATA_KEY = "last_chatted";
            var uplayer = UPlayer.From(player);

            if (!uplayer.Metadata.Has(METADATA_KEY)) {
                uplayer.Metadata[METADATA_KEY] = DateTime.Now;
                return;
            }

            var interval = UEssentials.Config.AntiSpam.Interval;

            if ((DateTime.Now - uplayer.Metadata.Get<DateTime>(METADATA_KEY)).TotalSeconds < interval) {
                EssLang.Send(uplayer, "CHAT_ANTI_SPAM");
                cancel = true;
                return;
            }

            uplayer.Metadata[METADATA_KEY] = DateTime.Now;
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
            CommandTell.ReplyTo.Remove(playerId);
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void GenericPlayerDeath(UnturnedPlayer rocketPlayer, EDeathCause c, ELimb l, CSteamID k) {
            const string METADATA_KEY = "KEEP_SKILL";
            const string KEEP_SKILL_PERM = "essentials.keepskill.";

            var player = UPlayer.From(rocketPlayer);
            var allPercentage = -1; // essentials.keepskill.all.<percentage>

            // Save skills in metadata, it will be restored when player respawn.
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
            // TODO: We should cache this. We need to find a way to detect when permissions change
            // and then re-compute this.
            foreach (var perm in player.Permissions.Where(perm => perm.StartsWith(KEEP_SKILL_PERM))) {
                var kind = perm.Substring(KEEP_SKILL_PERM.Length);
                var percentageToKeep = 100;

                if (string.IsNullOrEmpty(kind)) {
                    continue;
                }

                // Parse percentage, if present.
                // e.g 'essentials.keepskill.cardio.25' -> keepPercentage = 25
                if (kind.IndexOf('.') >= 0) {
                    // Split 'skill.percentage'
                    var parts = kind.Split('.');
                    if (!int.TryParse(parts[1], out percentageToKeep)) {
                        continue;
                    }
                    // Percentage must be between 0-100
                    if (percentageToKeep < 0) {
                        percentageToKeep = 0;
                    }
                    if (percentageToKeep > 100) {
                        percentageToKeep = 100;
                    }
                    kind = parts[0]; // let only skill name
                }

                if (kind.EqualsIgnoreCase("all")) {
                    allPercentage = percentageToKeep;
                    continue;
                }

                // Parse skill from name
                if (!USkill.FromName(kind, out var skill)) {
                    continue;
                }
                skillsToRestore[skill] = (byte) System.Math.Ceiling(player.GetSkillLevel(skill) * (percentageToKeep / 100.0));
            }

            // All Skills
            if (allPercentage != -1) {
                foreach (var skill in USkill.Skills) {
                    // We don't want change previously added (skillsToRestore) skills.
                    // This will allow to set a separated percentage while using modifier 'all' (essentials.keepskill.all)
                    // e.g
                    // essentials.keepskill.all.50
                    // essentials.keepskill.cardio.100
                    // this will keep 50% of all skills and 100% of cardio skill
                    if (skillsToRestore.ContainsKey(skill)) {
                        continue;
                    }
                    skillsToRestore[skill] = (byte) System.Math.Ceiling(player.GetSkillLevel(skill) * (allPercentage / 100.0));
                }
            }
        }

        [SubscribeEvent(EventType.PLAYER_REVIVE)]
        private void OnPlayerRespawn(UnturnedPlayer rocketPlayer, Vector3 l, byte s) {

            var player = UPlayer.From(rocketPlayer);
            var skillsToRestore = player.Metadata.GetOrDefault<Dictionary<USkill, byte>>("KEEP_SKILL", null);

            if (skillsToRestore != null) {
                foreach (var pair in skillsToRestore) {
                    player.SetSkillLevel(pair.Key, pair.Value);
                }
                skillsToRestore.Clear();
            }
        }

        private DateTime _lastUpdateCheck = DateTime.Now;

        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        private void JoinMessage(UnturnedPlayer player) {
            EssLang.BetterBroadcast("PLAYER_JOINEDICON", "PLAYER_JOINED", player.CharacterName);
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void LeaveMessage(UnturnedPlayer player) {
            EssLang.BetterBroadcast("PLAYER_EXITEDICON", "PLAYER_EXITED", player.CharacterName);
        }

        [SubscribeEvent(EventType.ESSENTIALS_COMMAND_PRE_EXECUTED)]
        private void OnCommandPreExecuted(CommandPreExecuteEvent e) {
            var commandName = e.Command.Name.ToLowerInvariant();

            if (
                e.Source.IsConsole ||
                !EssCore.Instance.CommandOptions.Commands.TryGetValue(commandName, out var commandOptions)
            ) return;

            // Check cooldown
            if (!e.Source.HasPermission("essentials.bypass.commandcooldown")) {
                var playerId = e.Source.ToPlayer().CSteamId.m_SteamID;

                if (
                    CommandCooldowns.ContainsKey(playerId) &&
                    CommandCooldowns[playerId].TryGetValue(commandName, out var nextUse) &&
                    nextUse > DateTime.Now
                ) {
                    var diffSec = (uint) (nextUse - DateTime.Now).TotalSeconds;
                    EssLang.Send(e.Source, "COMMAND_COOLDOWN", TimeUtil.FormatSeconds(diffSec));
                    e.Cancelled = true;
                    return;
                }
            }

            // Check if player has money enough to run this command
            if (UEssentials.EconomyProvider.IsPresent && !e.Source.HasPermission("essentials.bypass.commandcost")) {
                var cost = GetCommandCost(commandOptions, e.Source.ToPlayer());
                var ecoProvider = UEssentials.EconomyProvider.Value;

                if (cost > 0 && !ecoProvider.Has(e.Source.ToPlayer(), cost)) {
                    EssLang.Send(e.Source, "COMMAND_NO_MONEY", cost, ecoProvider.CurrencySymbol);
                    e.Cancelled = true;
                }
            }
        }

        private decimal GetCommandCost(CommandOptions.CommandEntry commandOptions, UPlayer player) {
            var cost = commandOptions.Cost;

            if (commandOptions.PerGroupCost != null) {
                R.Permissions.GetGroups(player.RocketPlayer, false)
                    .OrderBy(g => -g.Priority)
                    .FirstOrDefault(g => {
                        // Check if there is a cost specified to the player's group.
                        var result = commandOptions.PerGroupCost.TryGetValue(g.Id, out var groupCost);
                        // If there is, use that cost
                        if (result) cost = groupCost;
                        return result;
                    });
            }

            return cost;
        }

        [SubscribeEvent(EventType.ESSENTIALS_COMMAND_POS_EXECUTED)]
        private void OnCommandPosExecuted(CommandPosExecuteEvent e) {
            if (
                e.Source.IsConsole ||
                // It will only apply cooldown/cost if the command was sucessfully executed.
                e.Result.Type != CommandResult.ResultType.SUCCESS ||
                // Make sure there is options for the command being executed
                !EssCore.Instance.CommandOptions.Commands.TryGetValue(e.Command.Name.ToLowerInvariant(), out var commandOptions)
            ) return;

            HandleCooldown(e, commandOptions);
            HandleCost(e, commandOptions);
        }

        private void HandleCost(CommandPosExecuteEvent e, CommandOptions.CommandEntry commandOptions) {
            // Make sure it has an EconomyProvider and check if the player can bypass the cost
            if (!UEssentials.EconomyProvider.IsPresent || e.Source.HasPermission("essentials.bypass.commandcost")) {
                return;
            }
            var commandCost = GetCommandCost(commandOptions, e.Source.ToPlayer());
            if (commandCost > 0) {
                UEssentials.EconomyProvider.Value.Withdraw(e.Source.ToPlayer(), commandCost);
                EssLang.Send(e.Source, "COMMAND_PAID", commandCost, UEssentials.EconomyProvider.Value.CurrencySymbol);
            }
        }

        private void HandleCooldown(CommandPosExecuteEvent e, CommandOptions.CommandEntry commandOptions) {
            var commandName = e.Command.Name.ToLowerInvariant();

            // Check if the player can bypass the cooldown
            if (e.Source.HasPermission("essentials.bypass.commandcooldown")) {
                return;
            }

            var playerId = e.Source.ToPlayer().CSteamId.m_SteamID;
            var cooldownValue = commandOptions.Cooldown;

            if (commandOptions.PerGroupCooldown != null) {
                R.Permissions.GetGroups(e.Source.ToPlayer().RocketPlayer, false)
                    .OrderBy(g => -g.Priority)
                    .FirstOrDefault(g => {
                        // Check if there is a cooldown specified to the player's group.
                        var result = commandOptions.PerGroupCooldown.TryGetValue(g.Id, out var groupCooldown);
                        // If there is, use that cooldown.
                        if (result) cooldownValue = groupCooldown;
                        return result;
                    });
            }

            if (cooldownValue < 1) {
                return;
            }

            if (!CommandCooldowns.ContainsKey(playerId)) {
                CommandCooldowns.Add(playerId, new Dictionary<string, DateTime>());
            }

            CommandCooldowns[playerId][commandName] = DateTime.Now.AddSeconds(cooldownValue);
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
        private void HomePlayerMove(UnturnedPlayer player, Vector3 _) {
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
        private void BackPlayerDeath(UnturnedPlayer player, EDeathCause c, ELimb l, CSteamID k) {
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
        private void FreezePlayerDeath(UnturnedPlayer player, EDeathCause c, ELimb l, CSteamID k) {
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

    }

}
