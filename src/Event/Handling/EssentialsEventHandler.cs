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
using Essentials.Components.Player;
using Essentials.Core;
using Essentials.Economy;
using Essentials.I18n;
using Essentials.NativeModules.Kit;
using Essentials.NativeModules.Kit.Commands;
using Essentials.NativeModules.Warp.Commands;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;

namespace Essentials.Event.Handling {

    internal class EssentialsEventHandler {

        internal static readonly Dictionary<ulong, DateTime> LastChatted = new Dictionary<ulong, DateTime>();

        internal static readonly Dictionary<ulong, Dictionary<USkill, byte>> CachedSkills =
            new Dictionary<ulong, Dictionary<USkill, byte>>();

        [SubscribeEvent(EventType.PLAYER_CHATTED)]
        private void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message,
            EChatMode mode, ref bool cancel) {
            if (!UEssentials.Config.AntiSpam.Enabled || message.StartsWith("/") ||
                player.HasPermission("essentials.bypass.antispam")) return;

            var playerId = player.CSteamID.m_SteamID;

            if (!LastChatted.ContainsKey(playerId)) {
                LastChatted.Add(playerId, DateTime.Now);
                return;
            }

            var interval = UEssentials.Config.AntiSpam.Interval;

            if ((DateTime.Now - LastChatted[playerId]).TotalSeconds < interval) {
                EssLang.Send(UPlayer.From(playerId), "CHAT_ANTI_SPAM");
                cancel = true;
                return;
            }

            LastChatted[playerId] = DateTime.Now;
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void GenericPlayerDisconnected(UnturnedPlayer player) {
            var displayName = player.CharacterName;

            MiscCommands.Spies.Remove(player.CSteamID.m_SteamID);
            CommandTell.Conversations.Remove(player.CSteamID.m_SteamID);
            CachedSkills.Remove(player.CSteamID.m_SteamID);
            CommandHome.Cooldown.RemoveEntry(player.CSteamID);

            /* Kit Stuffs */
            UEssentials.ModuleManager.GetModule<KitModule>().IfPresent(m => {
                if (CommandKit.Cooldowns.Count == 0) return;
                if (!CommandKit.Cooldowns.ContainsKey(player.CSteamID.m_SteamID)) return;

                var playerId = player.CSteamID.m_SteamID;
                var playerCooldowns = CommandKit.Cooldowns[playerId];
                var keys = new List<string>(playerCooldowns.Keys);

                /*
                    Remove from list if cooldown has expired.

                    Global and per kit
                */
                var delta = CommandKit.GlobalCooldown[playerId].AddSeconds(UEssentials.Config.Kit.GlobalCooldown);

                if (CommandKit.GlobalCooldown.ContainsKey(playerId) && delta < DateTime.Now) {
                    CommandKit.GlobalCooldown.Remove(playerId);
                }

                foreach (var kitName in keys) {
                    var kit = m.KitManager.GetByName(kitName);

                    if (kit == null || playerCooldowns[kitName].AddSeconds(kit.Cooldown) < DateTime.Now) {
                        playerCooldowns.Remove(kitName);
                    }
                }

                if (playerCooldowns.Count == 0) {
                    CommandKit.Cooldowns.Remove(playerId);
                }
            });
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void GenericPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            var uplayer = UPlayer.From(player);

            CommandHome.Cooldown.RemoveIfExpired(player.CSteamID);

            /* Keep skill */
            const string KEEP_SKILL_PERM = "essentials.keepskill.";

            var globalPercentage = -1;
            var playerPermissions = player.GetPermissions().Select(p => p.Name).ToList();

            /*
                Search for 'global percentage' Ex: (essentials.keepskill.50)
            */

            foreach (var p in playerPermissions.Where(p => p.StartsWith(KEEP_SKILL_PERM))) {
                var rawAmount = p.Substring(KEEP_SKILL_PERM.Length);

                if (rawAmount.Equals("*")) {
                    globalPercentage = 100;
                    break;
                }
                if (int.TryParse(rawAmount, out globalPercentage)) {
                    break;
                }
                globalPercentage = -1;
            }

            /*
                If player has global percentage he will keep all skills.
            */
            var hasGlobalPercentage = (globalPercentage != -1 || player.IsAdmin);
            var skillValues = new Dictionary<USkill, byte>();

            foreach (var skill in USkill.Skills) {
                var currentLevel = uplayer.GetSkillLevel(skill);
                var newLevel = (byte?) null;

                if (hasGlobalPercentage) {
                    newLevel = (byte) Math.Round((currentLevel*globalPercentage)/100.0);
                    goto add;
                }

                /*
                    Search for single percentage.
                */
                var skillPermission = KEEP_SKILL_PERM + skill.Name.ToLowerInvariant() + ".";
                var skillPermission2 = KEEP_SKILL_PERM + skill.Name.ToLowerInvariant();
                var skillPercentage = -1;

                foreach (var p in playerPermissions) {
                    if (!p.StartsWith(skillPermission)) {
                        continue;
                    }

                    var rawAmount = p.Substring(skillPermission.Length);

                    if (int.TryParse(rawAmount, out skillPercentage)) {
                        break;
                    }

                    skillPercentage = -1;
                }

                if (skillPercentage != -1) {
                    newLevel = (byte) Math.Round((currentLevel*skillPercentage)/100.0);
                }

                /*
                    Ccheck for 'essentials.keepskill.SKILL'
                */
                if (!newLevel.HasValue && player.HasPermission(skillPermission2)) {
                    newLevel = currentLevel;
                }

                add:
                if (newLevel.HasValue) {
                    skillValues.Add(skill, newLevel.Value);
                }
            }

            if (skillValues.Count == 0) return;

            var id = player.CSteamID.m_SteamID;
            if (CachedSkills.ContainsKey(id)) {
                CachedSkills[id] = skillValues;
            } else {
                CachedSkills.Add(id, skillValues);
            }
        }

        [SubscribeEvent(EventType.PLAYER_REVIVE)]
        private void OnPlayerRespawn(UnturnedPlayer player, Vector3 vect, byte angle) {
            var playerId = player.CSteamID.m_SteamID;
            if (!CachedSkills.ContainsKey(playerId)) {
                return;
            }

            var uplayer = UPlayer.From(playerId);
            CachedSkills[playerId].ForEach(pair => {
                uplayer.SetSkillLevel(pair.Key, pair.Value);
            });
        }

        private DateTime _lastUpdateCheck = DateTime.Now;

        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        private void UpdateAlert(UnturnedPlayer player) {
            if (!player.IsAdmin || _lastUpdateCheck > DateTime.Now) return;

            var updater = EssCore.Instance.Updater;

            if (!updater.IsUpdated()) {
                _lastUpdateCheck = DateTime.Now.AddMinutes(10);

                Tasks.New(t => {
                    UPlayer.From(player).SendMessage("[uEssentials] New version avalaible " +
                                                     $"{updater.LastResult.LatestVersion}!", Color.cyan);
                }).Delay(1000).Go();
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

        private static Optional<IEconomyProvider> _cachedEconomyProvider;

        /*
            TODO: Cache commands & cost ??
        */

        [SubscribeEvent(EventType.ESSENTIALS_COMMAND_PRE_EXECUTED)]
        private void OnCommandPreExecuted(CommandPreExecuteEvent e) {
            if (e.Source.IsConsole || e.Source.HasPermission("essentials.bypass.commandcost")) {
                return;
            }

            if (_cachedEconomyProvider == null) {
                _cachedEconomyProvider = UEssentials.EconomyProvider;
            }

            if (_cachedEconomyProvider.IsAbsent) {
                /*
                    If economy hook is not present, this "handler" will be unregistered.
                */
                EssCore.Instance.EventManager.Unregister(GetType(), nameof(OnCommandPreExecuted));
                EssCore.Instance.EventManager.Unregister(GetType(), nameof(OnCommandPosExecuted));
            }

            var commands = EssCore.Instance.CommandsConfig.Commands;

            if (!commands.ContainsKey(e.Command.Name)) {
                return;
            }

            var cost = commands[e.Command.Name].Cost;

            if (cost <= 0) {
                return;
            }

            /*
                Check if player has sufficient money to use this command.
            */
            if (_cachedEconomyProvider.Value.Has(e.Source.ToPlayer(), cost)) {
                return;
            }

            EssLang.Send(e.Source, "COMMAND_NO_MONEY", cost, _cachedEconomyProvider.Value.Currency);
            e.Cancelled = true;
        }

        [SubscribeEvent(EventType.ESSENTIALS_COMMAND_POS_EXECUTED)]
        private void OnCommandPosExecuted(CommandPosExecuteEvent e) {
            if (_cachedEconomyProvider == null ||
                e.Source.IsConsole || e.Source.HasPermission("essentials.bypass.commandcost")) {
                return;
            }

            var commands = EssCore.Instance.CommandsConfig.Commands;

            /*
               He will not charge if command was not successfully executed.
            */
            if (e.Result?.Type != CommandResult.ResultType.SUCCESS ||
                !commands.ContainsKey(e.Command.Name)) {
                return;
            }

            var cost = commands[e.Command.Name].Cost;

            if (cost <= 0) {
                return;
            }

            _cachedEconomyProvider.Value.Withdraw(e.Source.ToPlayer(), cost);
            EssLang.Send(e.Source, "COMMAND_PAID", cost);
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void DeathMessages(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID killer) {
            switch (cause) {
                case EDeathCause.BLEEDING:
                    EssLang.Broadcast("DEATH_BLEEDING", player.CharacterName);
                    break;

                case EDeathCause.BONES:
                    EssLang.Broadcast("DEATH_BONES", player.CharacterName);
                    break;

                case EDeathCause.FREEZING:
                    EssLang.Broadcast("DEATH_FREEZING", player.CharacterName);
                    break;

                case EDeathCause.BURNING:
                    EssLang.Broadcast("DEATH_BURNING", player.CharacterName);
                    break;

                case EDeathCause.FOOD:
                    EssLang.Broadcast("DEATH_FOOD", player.CharacterName);
                    break;

                case EDeathCause.WATER:
                    EssLang.Broadcast("DEATH_WATER", player.CharacterName);
                    break;

                case EDeathCause.GUN:
                    var pKiller = UPlayer.From(killer)?.CharacterName ?? "?";
                    EssLang.Broadcast("DEATH_GUN", player.CharacterName, TranslateLimb(limb), pKiller);
                    break;

                case EDeathCause.MELEE:
                    pKiller = UPlayer.From(killer)?.CharacterName ?? "?";
                    EssLang.Broadcast("DEATH_MELEE", player.CharacterName, TranslateLimb(limb), pKiller);
                    break;

                case EDeathCause.ZOMBIE:
                    EssLang.Broadcast("DEATH_ZOMBIE", player.CharacterName);
                    break;

                case EDeathCause.ANIMAL:
                    EssLang.Broadcast("DEATH_ANIMAL", player.CharacterName);
                    break;

                case EDeathCause.SUICIDE:
                    EssLang.Broadcast("DEATH_SUICIDE", player.CharacterName);
                    break;

                case EDeathCause.KILL:
                    EssLang.Broadcast("DEATH_KILL", player.CharacterName);
                    break;

                case EDeathCause.INFECTION:
                    EssLang.Broadcast("DEATH_INFECTION", player.CharacterName);
                    break;

                case EDeathCause.PUNCH:
                    pKiller = UPlayer.From(killer)?.CharacterName ?? "?";
                    EssLang.Broadcast("DEATH_PUNCH", player.CharacterName, TranslateLimb(limb), pKiller);
                    break;

                case EDeathCause.BREATH:
                    EssLang.Broadcast("DEATH_BREATH", player.CharacterName);
                    break;

                case EDeathCause.ROADKILL:
                    pKiller = UPlayer.From(killer)?.CharacterName ?? "?";
                    EssLang.Broadcast("DEATH_ROADKILL", pKiller, player.CharacterName);
                    break;

                case EDeathCause.VEHICLE:
                    EssLang.Broadcast("DEATH_VEHICLE", player.CharacterName);
                    break;

                case EDeathCause.GRENADE:
                    EssLang.Broadcast("DEATH_GRENADE", player.CharacterName);
                    break;

                case EDeathCause.SHRED:
                    EssLang.Broadcast("DEATH_SHRED", player.CharacterName);
                    break;

                case EDeathCause.LANDMINE:
                    EssLang.Broadcast("DEATH_LANDMINE", player.CharacterName);
                    break;

                case EDeathCause.ARENA:
                    EssLang.Broadcast("DEATH_ARENA", player.CharacterName);
                    break;

                //TODO add on lang
                case EDeathCause.MISSILE:
                    break;

                case EDeathCause.CHARGE:
                    break;

                case EDeathCause.SPLASH:
                    break;

                case EDeathCause.SENTRY:
                    break;

                case EDeathCause.ACID:
                    break;
            }
        }

        /* Commands eventhandlers */

        [SubscribeEvent(EventType.PLAYER_UPDATE_POSITION)]
        private void HomePlayerMove(UnturnedPlayer player, Vector3 newPosition) {
            if (!UEssentials.Config.HomeCommand.CancelTeleportWhenMove || !CommandHome.Delay.ContainsKey(player.CSteamID.m_SteamID)) {
                return;
            }

            CommandHome.Delay[player.CSteamID.m_SteamID].Cancel();
            CommandHome.Delay.Remove(player.CSteamID.m_SteamID);
            CommandHome.Cooldown.RemoveEntry(player.CSteamID);

            UPlayer.TryGet(player, p => {
                EssLang.Send(p, "TELEPORT_CANCELLED_MOVED");
            });
        }

        [SubscribeEvent(EventType.PLAYER_UPDATE_POSITION)]
        private void WarpPlayerMove(UnturnedPlayer player, Vector3 newPosition) {
            if (!UEssentials.Config.WarpCommand.CancelTeleportWhenMove || !CommandWarp.Delay.ContainsKey(player.CSteamID.m_SteamID)) {
                return;
            }

            CommandWarp.Delay[player.CSteamID.m_SteamID].Cancel();
            CommandWarp.Delay.Remove(player.CSteamID.m_SteamID);

            UPlayer.TryGet(player, p => {
                EssLang.Send(p, "TELEPORT_CANCELLED_MOVED");
            });
        }

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void BackPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            if (!player.HasPermission("essentials.command.back")) {
                return;
            }

            UPlayer.TryGet(player, p => p.SetMetadata("back_pos", player.Position));
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

        [SubscribeEvent(EventType.PLAYER_DEATH)]
        private void KitPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            var globalKitCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;

            if (CommandKit.GlobalCooldown.ContainsKey(player.CSteamID.m_SteamID) && EssCore.Instance.Config.Kit.ResetGlobalCooldownWhenDie) {
                CommandKit.GlobalCooldown[player.CSteamID.m_SteamID] = DateTime.Now.AddSeconds(-globalKitCooldown);
            }

            if (!CommandKit.Cooldowns.ContainsKey(player.CSteamID.m_SteamID)) return;

            var playerCooldowns = CommandKit.Cooldowns[player.CSteamID.m_SteamID];
            var keys = new List<string>(playerCooldowns.Keys);

            foreach (var kitName in keys) {
                var kit = KitModule.Instance.KitManager.GetByName(kitName);

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

        private static string TranslateLimb(ELimb limb) {
            switch (limb) {
                case ELimb.SKULL:
                    return EssLang.Translate("LIMB_HEAD");

                case ELimb.LEFT_HAND:
                case ELimb.RIGHT_HAND:
                case ELimb.LEFT_ARM:
                case ELimb.RIGHT_ARM:
                    return EssLang.Translate("LIMB_ARM");

                case ELimb.LEFT_FOOT:
                case ELimb.RIGHT_FOOT:
                case ELimb.LEFT_LEG:
                case ELimb.RIGHT_LEG:
                    return EssLang.Translate("LIMB_LEG");

                case ELimb.SPINE:
                    return EssLang.Translate("LIMB_TORSO");

                default:
                    return "?";
            }
        }

    }

}