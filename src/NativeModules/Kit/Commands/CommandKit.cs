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
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core;
using Essentials.I18n;
using System;
using System.Collections.Generic;

namespace Essentials.NativeModules.Kit.Commands
{

    [CommandInfo(
        Name = "kit",
        Description = "Get a kit",
        Usage = "[kit_name] <player | *>"
    )]
    public class CommandKit : EssCommand
    {

        /*
            player_id -> [kit_name, last_use]
        */
        internal static Dictionary<ulong, Dictionary<string, DateTime>> Cooldowns =
            new Dictionary<ulong, Dictionary<string, DateTime>>();

        internal static Dictionary<ulong, DateTime> GlobalCooldown =
            new Dictionary<ulong, DateTime>();

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.Length == 0 || (args.Length == 1 && src.IsConsole))
            {
                return CommandResult.ShowUsage();
            }

            var player = src.ToPlayer();
            var kitName = args[0].ToLowerString;

            if (!KitModule.Instance.KitManager.Contains(kitName))
            {
                return CommandResult.LangError("KIT_NOT_EXIST", kitName);
            }

            var requestedKit = KitModule.Instance.KitManager.GetByName(kitName);

            if (!requestedKit.CanUse(player))
            {
                return CommandResult.LangError("KIT_NO_PERMISSION");
            }

            var steamPlayerId = player.CSteamId.m_SteamID;
            var kitCost = requestedKit.Cost;

            if (
                kitCost > 0 &&
                UEssentials.EconomyProvider.IsPresent &&
                !src.HasPermission("essentials.bypass.kitcost")
            )
            {
                var ecoProvider = UEssentials.EconomyProvider.Value;

                if (!ecoProvider.Has(player, kitCost))
                {
                    return CommandResult.LangError("KIT_NO_MONEY", kitCost, ecoProvider.CurrencySymbol);
                }
            }

            var globalCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;
            var kitCooldown = requestedKit.Cooldown;

            if (!src.HasPermission("essentials.bypass.kitcooldown"))
            {
                // Check if is on global cooldown
                if (globalCooldown > 0 && GlobalCooldown.ContainsKey(steamPlayerId))
                {
                    var remainingTime = DateTime.Now - GlobalCooldown[steamPlayerId];

                    if ((remainingTime.TotalSeconds + 1) < globalCooldown)
                    {
                        return CommandResult.LangError("KIT_GLOBAL_COOLDOWN",
                            TimeUtil.FormatSeconds((uint)(globalCooldown - remainingTime.TotalSeconds)));
                    }
                }

                // Check if is on cooldown for this specific kit
                if (kitCooldown > 0)
                {
                    if (!Cooldowns.TryGetValue(steamPlayerId, out var playerCooldowns) || playerCooldowns == null)
                    {
                        Cooldowns[steamPlayerId] = playerCooldowns = new Dictionary<string, DateTime>();
                    }

                    if (playerCooldowns.TryGetValue(kitName, out var lastTimeUsedThisKit))
                    {
                        var remainingTime = DateTime.Now - lastTimeUsedThisKit;

                        if ((remainingTime.TotalSeconds + 1) < kitCooldown)
                        {
                            return CommandResult.LangError("KIT_COOLDOWN", TimeUtil.FormatSeconds(
                                (uint)(kitCooldown - remainingTime.TotalSeconds)));
                        }
                    }
                }
            }

            if (kitCost > 0 && !src.HasPermission("essentials.bypass.kitcost"))
            {
                UEssentials.EconomyProvider.IfPresent(ec =>
                {
                    ec.Withdraw(player, kitCost);
                    EssLang.Send(player, "KIT_PAID", kitCost, ec.CurrencySymbol);
                });
            }

            if (args.Length == 1)
            {

                requestedKit.GiveTo(player);

                // Only apply the cooldowns if the player received the kit
                // and does not have the bypass permission.
                if (!src.HasPermission("essentials.bypass.kitcooldown"))
                {
                    if (globalCooldown > 0) GlobalCooldown[steamPlayerId] = DateTime.Now;
                    if (kitCooldown > 0) Cooldowns[steamPlayerId][kitName] = DateTime.Now;
                }
            }
            else if (args.Length == 2)
            {

                if (!src.HasPermission($"essentials.kit.{kitName}.other"))
                {
                    return CommandResult.NoPermission($"essentials.kit.{kitName}.other");
                }

                if (!KitModule.Instance.KitManager.Contains(kitName))
                {
                    return CommandResult.LangError("KIT_NOT_EXIST", kitName);
                }

                var kit = KitModule.Instance.KitManager.GetByName(kitName);
                if (args[1].Equals("*"))
                {
                    if (player.IsAdmin || player.HasPermission("*"))
                    {
                        UServer.Players.ForEach(kit.GiveTo);
                        EssLang.Send(src, "KIT_GIVEN_SENDER_ALL", kitName);
                    }
                    else
                    {
                        return CommandResult.LangError("KIT_NO_PERMISSION");
                    }

                }
                else
                {
                    if (!UPlayer.TryGet(args[1].ToString(), out var target))
                    {
                        return CommandResult.LangError("PLAYER_NOT_FOUND", args[1]);
                    }

                    if (!src.HasPermission("essentials.bypass.kitcooldown") && !src.IsConsole)
                    {
                        if (globalCooldown > 0) GlobalCooldown[steamPlayerId] = DateTime.Now;
                        if (kitCooldown > 0) Cooldowns[steamPlayerId][kitName] = DateTime.Now;
                    }

                    kit.GiveTo(target);
                    EssLang.Send(src, "KIT_GIVEN_SENDER", kitName, target);
                }

            }

            return CommandResult.Success();
        }

    }
}