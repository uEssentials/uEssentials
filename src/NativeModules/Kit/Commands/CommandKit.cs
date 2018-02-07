#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using Essentials.I18n;
using Essentials.Core;
using Essentials.Api.Unturned;
using Essentials.Common;

namespace Essentials.NativeModules.Kit.Commands {

    [CommandInfo(
        Name = "kit",
        Description = "Get a kit",
        Usage = "[kit_name] <player | *>"
    )]
    public class CommandKit : EssCommand {

        /*
            player_id -> [kit_name, last_use]
        */
        internal static Dictionary<ulong, Dictionary<string, DateTime>> Cooldowns =
            new Dictionary<ulong, Dictionary<string, DateTime>>();

        internal static Dictionary<ulong, DateTime> GlobalCooldown =
            new Dictionary<ulong, DateTime>();

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.Length == 0 || (args.Length == 1 && src.IsConsole)) {
                return CommandResult.ShowUsage();
            }

            if (args.Length == 1) {
                var player = src.ToPlayer();

                if (!KitModule.Instance.KitManager.Contains(args[0].ToString())) {
                    return CommandResult.LangError("KIT_NOT_EXIST", args[0]);
                }

                var kitName = args[0].ToLowerString;
                var requestedKit = KitModule.Instance.KitManager.GetByName(kitName);

                if (!requestedKit.CanUse(player)) {
                    return CommandResult.LangError("KIT_NO_PERMISSION");
                }

                var steamPlayerId = player.CSteamId.m_SteamID;
                var kitCost = requestedKit.Cost;

                if (kitCost > 0 && UEssentials.EconomyProvider.IsPresent) {
                    var ecoProvider = UEssentials.EconomyProvider.Value;

                    if (!ecoProvider.Has(player, kitCost)) {
                        return CommandResult.LangError("KIT_NO_MONEY", kitCost, ecoProvider.CurrencySymbol);
                    }
                }

                if (!src.HasPermission("essentials.bypass.kitcooldown")) {
                    var globalCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;

                    if (globalCooldown > 0) {
                        if (GlobalCooldown.ContainsKey(steamPlayerId)) {
                            var remainingTime = DateTime.Now - GlobalCooldown[steamPlayerId];

                            if ((remainingTime.TotalSeconds + 1) > globalCooldown) {
                                GlobalCooldown[steamPlayerId] = DateTime.Now;
                            } else {
                                return CommandResult.LangError("KIT_GLOBAL_COOLDOWN",
                                    TimeUtil.FormatSeconds((uint) (globalCooldown - remainingTime.TotalSeconds)));
                            }
                        } else {
                            GlobalCooldown.Add(steamPlayerId, DateTime.Now);
                        }
                    } else {
                        var kitCooldown = requestedKit.Cooldown;

                        if (!Cooldowns.ContainsKey(steamPlayerId)) {
                            Cooldowns.Add(steamPlayerId, new Dictionary<string, DateTime>());
                        } else if (Cooldowns[steamPlayerId] == null) {
                            Cooldowns[steamPlayerId] = new Dictionary<string, DateTime>();
                        }

                        if (Cooldowns[steamPlayerId].ContainsKey(kitName)) {
                            var remainingTime = DateTime.Now - Cooldowns[steamPlayerId][kitName];

                            if ((remainingTime.TotalSeconds + 1) > kitCooldown) {
                                Cooldowns[steamPlayerId][kitName] = DateTime.Now;
                            } else {
                                return CommandResult.LangError("KIT_COOLDOWN", TimeUtil.FormatSeconds(
                                    (uint) (kitCooldown - remainingTime.TotalSeconds)));
                            }
                        } else {
                            Cooldowns[steamPlayerId].Add(kitName, DateTime.Now);
                        }
                    }
                }

                if (kitCost > 0) {
                    UEssentials.EconomyProvider.IfPresent(ec => {
                        ec.Withdraw(player, kitCost);
                        EssLang.Send(player, "KIT_PAID", kitCost, ec.CurrencySymbol);
                    });
                }

                requestedKit.GiveTo(player);
            } else if (args.Length == 2) {
                var kitName = args[0].ToLowerString;

                if (!src.HasPermission($"essentials.kit.{kitName}.other")) {
                    return CommandResult.Empty();
                }

                if (!KitModule.Instance.KitManager.Contains(kitName)) {
                    return CommandResult.LangError("KIT_NOT_EXIST", kitName);
                }

                var kit = KitModule.Instance.KitManager.GetByName(kitName);

                if (args[1].Equals("*")) {
                    UServer.Players.ForEach(kit.GiveTo);
                    EssLang.Send(src, "KIT_GIVEN_SENDER_ALL", kitName);
                } else {
                    var target = args[1].ToPlayer;

                    if (target == null) {
                        return CommandResult.LangError("PLAYER_NOT_FOUND", args[1]);
                    }

                    kit.GiveTo(target);
                    EssLang.Send(src, "KIT_GIVEN_SENDER", kitName, target);
                }
            }

            return CommandResult.Success();
        }

    }

}