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
using System.Threading;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Core.Command;
using Essentials.I18n;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Essentials.Common.Util;
using Essentials.Components.Player;

namespace Essentials.Commands {

    public class MiscCommands {

        private static readonly ICommandArgument One = new CommandArgument(0, "1");
        public static readonly HashSet<ulong> Spies = new HashSet<ulong>();

        [CommandInfo(
            Name = "ascend",
            Aliases = new[] { "asc" },
            Usage = "[amount]",
            Description = "Ascend X \"meters\".",
            AllowedSource = AllowedSource.PLAYER
        )]
        private CommandResult AscendCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (args.IsEmpty) {
                return CommandResult.ShowUsage();
            }

            if (!args[0].IsFloat) {
                return CommandResult.Lang(EssLang.INVALID_NUMBER, args[0]);
            }

            if (args[0].ToFloat <= 0) {
                return CommandResult.Lang(EssLang.MUST_POSITIVE, args[0]);
            }

            var player = src.ToPlayer();
            var pos = new Vector3(player.Position.x, player.Position.y, player.Position.z);
            var num = args[0].ToFloat;

            pos.y += num;

            player.Teleport(pos);
            EssLang.ASCENDED.SendTo(src, num);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "descend",
            Aliases = new[] { "desc" },
            Usage = "[amount]",
            Description = "Descend X \"meters\".",
            AllowedSource = AllowedSource.PLAYER
        )]
        private CommandResult DescendCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (args.IsEmpty) {
                return CommandResult.ShowUsage();
            }

            if (!args[0].IsFloat) {
                return CommandResult.Lang(EssLang.INVALID_NUMBER, args[0]);
            }

            if (args[0].ToFloat <= 0) {
                return CommandResult.Lang(EssLang.MUST_POSITIVE, args[0]);
            }

            var player = src.ToPlayer();
            var pos = new Vector3(player.Position.x, player.Position.y, player.Position.z);
            var num = args[0].ToFloat;

            pos.y -= num;

            player.Teleport(pos);
            EssLang.DESCENDED.SendTo(src, num);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "clear",
            Description = "Clear things",
            Usage = "[i = items, v = vehicles, ev = empty vehicles] <distance>"
        )]
        private CommandResult ClearCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (args.IsEmpty) {
                return CommandResult.ShowUsage();
            }

            /*
                TODO: Options
                    -i = items
                    -v = vehicles
                    -z = zombies
                    -b = barricades
                    -s = structures
                    -a = ALL

                /clear -i -z -v = items, zombies, vehicles
            */

            var distance = -1;

            if (args.Length > 1) {
                if (src.IsConsole) {
                    return CommandResult.ShowUsage();
                }

                if (!args[1].IsInt) {
                    return CommandResult.Lang(EssLang.INVALID_NUMBER, args[1]);
                }

                if (args[1].ToInt < 1) {
                    return CommandResult.Lang(EssLang.NUMBER_BETWEEN, 1, int.MaxValue);
                }

                distance = args[1].ToInt;
            }

            switch (args[0].ToLowerString) {
                case "ev":
                case "emptyvehicles":
                    if (!src.HasPermission(cmd.Permission + ".emptyvehicles")) {
                        return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                    }

                    new Thread(() => {
                        var toRemove = new List<uint>();

                        UWorld.Vehicles
                            .Where(v => v.passengers.All(p => p?.player == null))
                            .Where(v => {
                                if (distance == -1) return true;
                                return Vector3.Distance(v.transform.position, src.ToPlayer().Position) <= distance;
                            })
                            .Select(v => v.instanceID)
                            .ForEach(toRemove.Add);

                        toRemove.ForEach(id => {
                            VehicleManager.Instance.SteamChannel.send("tellVehicleDestroy",
                                ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, id);
                        });

                        EssLang.CLEAR_EMPTY_VEHICLES.SendTo(src, toRemove.Count);
                    }).Start();
                    break;

                case "v":
                case "vehicle":
                    return CommandResult.Generic("This option is currently disabled, use /clear ev instead");
                    /*
                    if (!src.HasPermission(cmd.Permission + ".vehicles")) {
                        return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                    }

                    new Thread(() => {
                        UWorld.Vehicles.ForEach(v => {
                            for (byte i = 0; i < v.passengers.Length; i++) {
                                if (v.passengers[i] == null ||
                                    v.passengers[i].player == null) continue;

                                Vector3 point;
                                byte angle;

                                v.getExit(i, out point, out angle);
                                VehicleManager.sendExitVehicle(v, i, point, angle, true);
                            }
                        });

                        VehicleManager.askVehicleDestroyAll();
                        EssLang.CLEAR_VEHICLES.SendTo(src);
                    }).Start();
                    break;*/

                case "i":
                case "items":
                    if (!src.HasPermission(cmd.Permission + ".items")) {
                        return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                    }

                    ItemManager.askClearAllItems();
                    EssLang.CLEAR_ITEMS.SendTo(src);
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "item",
            Description = "Give an item to you/another player",
            Usage = "[item] <amount> or [player|* = all] [item] [amount]",
            Aliases = new[] { "i" }
        )]
        private CommandResult ItemCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            switch (args.Length) {
                /*
                    /i [item]
                 */
                case 1:
                    if (src.IsConsole) {
                        return CommandResult.ShowUsage();
                    }
                    GiveItem(src, src.ToPlayer(), args[0], One);
                    break;

                /*
                    /i [item] [amount]
                    /i [player] [item]
                    /i all [item]
                 */
                case 2:
                    if (args[1].IsInt) {
                        if (src.IsConsole) {
                            return CommandResult.ShowUsage();
                        }
                        GiveItem(src, src.ToPlayer(), args[0], args[1]);
                    } else if (args[0].Is("*")) {
                        GiveItem(src, null, args[1], One, true);
                    } else if (!args[0].IsValidPlayerName) {
                        return CommandResult.Lang(EssLang.PLAYER_NOT_FOUND, args[0]);
                    } else {
                        GiveItem(src, UPlayer.From(args[0].ToString()), args[1], One);
                    }
                    break;

                /*
                    /i [player] [item] [amount]
                    /i all [item] [amount]
                 */
                case 3:
                    if (args[0].Is("*")) {
                        GiveItem(src, null, args[1], args[2], true);
                    } else if (!args[0].IsValidPlayerName) {
                        return CommandResult.Lang(EssLang.PLAYER_NOT_FOUND, args[0]);
                    } else {
                        GiveItem(src, UPlayer.From(args[0].ToString()), args[1], args[2]);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "iteminfo",
            Aliases = new[] { "ii" },
            Description = "See information on an item.",
            Usage = "<item_id>"
        )]
        private CommandResult ItemInfoCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (src.IsConsole && args.Length != 1) {
                return CommandResult.ShowUsage();
            }

            ItemAsset asset;

            if (args.Length == 0) {
                var equipament = src.ToPlayer().Equipment;

                if (equipament.HoldingItemID == 0) {
                    return CommandResult.Lang(EssLang.EMPTY_HANDS);
                }

                asset = equipament.asset;
            } else {
                if (!args[0].IsUshort ||
                    (asset = Assets.find(EAssetType.ITEM, args[0].ToUshort) as ItemAsset) == null) {
                    return CommandResult.Lang(EssLang.INVALID_ITEM_ID, args[0]);
                }
            }

            var color = Color.yellow;
            var name = WrapMessage(src, asset.name);
            var description = WrapMessage(src, asset.Description);
            var type = WrapMessage(src, asset.ItemType.ToString());

            src.SendMessage($"Name: {name}", color);
            src.SendMessage($"Description: {description}", color);
            src.SendMessage($"Id: {asset.id}", color);
            src.SendMessage($"Type: {type}", color);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "itemfeatures",
            Aliases = new[] { "if" },
            Usage = "[autoreload | autorepair] [on | off]",
            Description = "Item features",
            AllowedSource = AllowedSource.PLAYER
        )]
        private CommandResult ItemFeaturesCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (args.Length != 2) {
                return CommandResult.ShowUsage();
            }

            bool toggleValue;

            if (args[1].IsOneOf(new[] { "1", "on", "true" })) {
                toggleValue = true;
            } else if (args[1].IsOneOf(new[] { "0", "off", "false" })) {
                toggleValue = false;
            } else {
                return CommandResult.ShowUsage();
            }

            var player = src.ToPlayer();
            var component = player.GetComponent<ItemFeatures>() ?? player.AddComponent<ItemFeatures>();

            switch (args[0].ToLowerString) {
                case "autoreload":
                    if (!src.HasPermission($"{cmd.Permission}.autoreload")) {
                        return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                    }
                    if (toggleValue) {
                        component.AutoReload = true;
                        EssLang.AUTO_RELOAD_ENABLED.SendTo(src);
                    } else {
                        component.AutoReload = false;
                        EssLang.AUTO_RELOAD_DISABLED.SendTo(src);
                    }
                    break;

                case "autorepair":
                    if (!src.HasPermission($"{cmd.Permission}.autorepair")) {
                        return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                    }
                    if (toggleValue) {
                        component.AutoRepair = true;
                        EssLang.AUTO_REPAIR_ENABLED.SendTo(src);
                    } else {
                        component.AutoRepair = false;
                        EssLang.AUTO_REPAIR_DISABLED.SendTo(src);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "vehiclefeatures",
            Aliases = new[] { "vehfeatures", "vf" },
            Usage = "[autorefuel | autorepair] [on|off]",
            Description = "Vehicle features",
            AllowedSource = AllowedSource.PLAYER
        )]
        private CommandResult VehicleFeaturesCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (args.Length != 2) {
                return CommandResult.ShowUsage();
            }

            bool toggleValue;

            if (args[1].IsOneOf(new[] { "1", "on", "true" })) {
                toggleValue = true;
            } else if (args[1].IsOneOf(new[] { "0", "off", "false" })) {
                toggleValue = false;
            } else {
                return CommandResult.ShowUsage();
            }

            var player = src.ToPlayer();
            var component = player.GetComponent<VehicleFeatures>() ?? player.AddComponent<VehicleFeatures>();

            switch (args[0].ToLowerString) {
                case "autorefuel":
                    if (toggleValue) {
                        component.AutoRefuel = true;
                        EssLang.AUTO_REFUEL_ENABLED.SendTo(src);
                    } else {
                        component.AutoRefuel = false;
                        EssLang.AUTO_REFUEL_DISABLED.SendTo(src);
                    }
                    break;

                case "autorepair":
                    if (toggleValue) {
                        component.AutoRepair = true;
                        EssLang.AUTO_REPAIR_ENABLED.SendTo(src);
                    } else {
                        component.AutoRepair = false;
                        EssLang.AUTO_REPAIR_DISABLED.SendTo(src);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "spypm",
            Description = "Allows you to see private messages.",
            AllowedSource = AllowedSource.PLAYER
        )]
        private CommandResult SpyCommand(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();
            var playerId = player.CSteamId.m_SteamID;

            if (Spies.Contains(playerId)) {
                Spies.Remove(playerId);
                EssLang.SPY_MODE_OFF.SendTo(src);
            } else {
                Spies.Add(playerId);
                EssLang.SPY_MODE_ON.SendTo(src);
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "suicide",
            Description = "Kill yourself",
            AllowedSource = AllowedSource.PLAYER
        )]
        private CommandResult SuicideCommand(ICommandSource src, ICommandArgs args) {
            src.ToPlayer().Suicide();

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "position",
            Aliases = new[] { "pos", "coords" },
            Description = "View your/another player position.",
            Usage = "<player>"
        )]
        private CommandResult PositionCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (args.Length == 0) {
                if (src.IsConsole) {
                    return CommandResult.ShowUsage();
                } else {
                    var player = src.ToPlayer();

                    EssLang.POSITION.SendTo(player,
                        player.Position.x,
                        player.Position.y,
                        player.Position.z);
                }
            } else {
                var found = UPlayer.TryGet(args[0], p => {
                    EssLang.POSITION_OTHER.SendTo(src, p.DisplayName, p.Position.x, p.Position.y, p.Position.z);
                });

                if (!found) {
                    return CommandResult.Lang(EssLang.PLAYER_NOT_FOUND, args[0]);
                }
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "online",
            Description = "View the number of online players"
        )]
        private CommandResult OnlineCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            EssLang.ONLINE_PLAYERS.SendTo(src, UServer.Players.Count(), UServer.MaxPlayers);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "respawnitems",
            Description = "Respawn all items."
        )]
        private CommandResult RespawnItemsCommand(ICommandSource src, ICommandArgs args) {
            for (byte b = 0; b < Regions.WORLD_SIZE; b += 1) {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 += 1) {
                    if (LevelItems.spawns[b, b2].Count <= 0) continue;

                    for (var i = 0; i < LevelItems.spawns[b, b2].Count; i++) {
                        var itemSpawnpoint = LevelItems.spawns[b, b2][i];
                        var item = LevelItems.getItem(itemSpawnpoint);

                        if (item == 0) continue;

                        var item2 = new Item(item, true);
                        ItemManager.dropItem(item2, itemSpawnpoint.point, false, false, false);
                    }
                }
            }

            EssLang.RESPAWNED_ITEMS.SendTo(src);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "respawnvehicles",
            Description = "Respawn all vehicles."
        )]
        private CommandResult RespawnVehiclesCommand(ICommandSource src, ICommandArgs args) {
            var spawns = LevelVehicles.spawns;

            foreach (var vehicleSpawnpoint in spawns) {
                var vehicle = LevelVehicles.getVehicle(vehicleSpawnpoint);

                if (vehicle == 0)
                    continue;

                var point = vehicleSpawnpoint.point;
                point.y += 1f;
                VehicleManager.spawnVehicle(vehicle, point, Quaternion.Euler(0f, vehicleSpawnpoint.angle, 0f));
            }

            EssLang.RESPAWNED_VEHICLES.SendTo(src);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "shutdown",
            Aliases = new[] { "stop" },
            Description = "Shutdown server",
            Usage = "<reason>"
        )]
        private CommandResult ShutdownCommand(ICommandSource src, ICommandArgs args) {
            if (!args.IsEmpty) {
                Commander.execute(CSteamID.Nil, "kickall " + args.Join(0));
            }

            Provider.shutdown();

            return CommandResult.Success();
        }


        // TODO: Wrap in spawned? (GTA STYLE)
        [CommandInfo(
            Name = "vehicle",
            Aliases = new[] { "v" },
            Description = "",
            Usage = "[vehicle] or [player|* = all] [vehicle]"
        )]
        private CommandResult VehicleCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            switch (args.Length) {
                case 1:
                    if (src.IsConsole) {
                        return CommandResult.ShowUsage();
                    }

                    var optAsset = VehicleUtil.GetVehicle(args[0].ToString());

                    if (optAsset.IsAbsent) {
                        return CommandResult.Lang(EssLang.INVALID_VEHICLE_ID, args[0]);
                    }

                    var id = optAsset.Value.id;

                    if (UEssentials.Config.VehicleBlacklist.Contains(id) &&
                        !src.HasPermission("essentials.bypass.blacklist.vehicle")) {
                        return CommandResult.Lang(EssLang.BLACKLISTED_VEHICLE, $"{optAsset.Value.vehicleName} ({id})");
                    }

                    VehicleTool.giveVehicle(src.ToPlayer().UnturnedPlayer, id);

                    EssLang.RECEIVED_VEHICLE.SendTo(src, optAsset.Value.Name, id);
                    break;

                case 2:
                    if (!src.HasPermission($"{cmd.Permission}.other")) {
                        return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                    }

                    optAsset = VehicleUtil.GetVehicle(args[1].ToString());

                    if (optAsset.IsAbsent) {
                        return CommandResult.Lang(EssLang.INVALID_VEHICLE_ID, args[1]);
                    }

                    var vehAsset = optAsset.Value;

                    if (args[0].Is("*")) {
                        UServer.Players.ForEach(p => {
                            VehicleTool.giveVehicle(p.UnturnedPlayer, vehAsset.id);
                        });

                        EssLang.GIVEN_VEHICLE_ALL.SendTo(src, vehAsset.Name, vehAsset.Id);
                    } else if (!args[0].IsValidPlayerName) {
                        return CommandResult.Lang(EssLang.PLAYER_NOT_FOUND, args[0]);
                    } else {
                        var target = args[0].ToPlayer;
                        VehicleTool.giveVehicle(target.UnturnedPlayer, vehAsset.id);

                        EssLang.GIVEN_VEHICLE.SendTo(src, vehAsset.Name, vehAsset.Id, target.DisplayName);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "systemtime",
            Aliases = new[] { "stime" },
            Description = "Show system time."
        )]
        private CommandResult SystemTimeCommand(ICommandSource src, ICommandArgs args) {
            src.SendMessage(DateTime.Now, Color.yellow);
            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "tps",
            Description = "Show tps."
        )]
        private CommandResult TpsCommand(ICommandSource src, ICommandArgs args) {
            Color color;
            var tps = Provider.debugTPS;

            if (tps > 40)
                color = Color.green;
            else if (tps < 40 && tps > 25)
                color = Color.yellow;
            else
                color = Color.red;

            src.SendMessage($"Ticks per second: {tps}", color);
            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "skill",
            Description = "Edit skill of an player|you",
            Usage = "[skill] [value|max] or [player|*] [skill] [value|max]"
        )]
        private CommandResult SkillCommand(ICommandSource src, ICommandArgs args) {
            switch (args.Length) {
                // /skill [skill] [value]
                case 2:
                    if (src.IsConsole) {
                        return CommandResult.ShowUsage();
                    }

                    var optSkill = USkill.FromName(args[0].ToString());

                    if (optSkill.IsAbsent) {
                        return CommandResult.Lang(EssLang.INVALID_SKILL, args[0]);
                    }

                    var player = src.ToPlayer();
                    byte value;

                    if (args[1].Is("max")) {
                        value = player.GetSkill(optSkill.Value).max;
                    } else if (args[1].IsInt) {
                        if (args[1].ToInt < 0 || args[1].ToInt > byte.MaxValue) {
                            return CommandResult.Lang(EssLang.NEGATIVE_OR_LARGE);
                        }

                        value = (byte) args[1].ToInt;
                    } else {
                        return CommandResult.Lang(EssLang.INVALID_NUMBER, args[1]);
                    }

                    player.SetSkillLevel(optSkill.Value, value);
                    EssLang.SKILL_SET.SendTo(src, optSkill.Value.Name.Capitalize(), args[1]);
                    break;

                // /skill [player|*] [skill] [value]
                case 3:
                    if (args[0].Is("*")) {
                        if (!UServer.Players.Any()) {
                            return CommandResult.Lang(EssLang.ANYONE_ONLINE);
                        }

                        player = UServer.Players.First();
                    } else {
                        if (!args[0].IsValidPlayerName) {
                            return CommandResult.Lang(EssLang.PLAYER_NOT_FOUND);
                        }

                        player = args[0].ToPlayer;
                    }

                    optSkill = USkill.FromName(args[1].ToString());

                    if (optSkill.IsAbsent) {
                        return CommandResult.Lang(EssLang.INVALID_SKILL, args[0]);
                    }

                    if (args[2].Is("max")) {
                        value = player.GetSkill(optSkill.Value).max;
                    } else if (args[2].IsInt) {
                        if (args[2].ToInt < 0 || args[2].ToInt > byte.MaxValue) {
                            return CommandResult.Lang(EssLang.NEGATIVE_OR_LARGE);
                        }

                        value = (byte) args[2].ToInt;
                    } else {
                        return CommandResult.Lang(EssLang.INVALID_NUMBER, args[2]);
                    }

                    if (args[0].Is("*")) {
                        UServer.Players.ForEach(p => p.SetSkillLevel(optSkill.Value, value));
                        EssLang.SKILL_SET_ALL.SendTo(src, optSkill.Value.Name.Capitalize(), args[2]);
                    } else {
                        player.SetSkillLevel(optSkill.Value, value);
                        EssLang.SKILL_SET_PLAYER.SendTo(src, optSkill.Value.Name.Capitalize(),
                            player.CharacterName, args[2]);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }
            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "pvp",
            Description = "Enable or disable pvp",
            Usage = "[on|off]"
        )]
        private CommandResult PvpCommand(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty) {
                return CommandResult.ShowUsage();
            }

            switch (args[0].ToLowerString) {
                case "on":
                case "true":
                case "yes":
                    Provider.PvP = true;
                    EssLang.PVP_ENABLED.SendTo(src);
                    break;

                case "off":
                case "false":
                case "no":
                    Provider.PvP = false;
                    EssLang.PVP_DISABLED.SendTo(src);
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

        #region HELPER METHODS

        private static string WrapMessage(ICommandSource src, string str) {
            if (str == null)
                return "null";

            if (str.Length < 90 || src.IsConsole)
                return str;

            return str.Substring(0, 90 - 3) + "...";
        }

        private static void GiveItem(ICommandSource src, UPlayer target, ICommandArgument itemArg,
            ICommandArgument amountArg, bool allPlayers = false) {
            if (!src.HasPermission("essentials.command.item.other") && target != src) {
                EssLang.COMMAND_NO_PERMISSION.SendTo(src);
                return;
            }

            var optAsset = ItemUtil.GetItem(itemArg.ToString());

            if (optAsset.IsAbsent) {
                EssLang.ITEM_NOT_FOUND.SendTo(src, itemArg);
                return;
            }

            if (UEssentials.Config.GiveItemBlacklist.Contains(optAsset.Value.id) &&
                !src.HasPermission("essentials.bypass.blacklist.item")) {
                EssLang.BLACKLISTED_ITEM.SendTo(src, $"{optAsset.Value.itemName} ({optAsset.Value.Id})");
                return;
            }

            ushort amt = 1;

            if (amountArg != null) {
                if (!amountArg.IsShort) {
                    EssLang.INVALID_NUMBER.SendTo(src, amountArg);
                } else if (amountArg.ToUshort <= 0) {
                    EssLang.MUST_POSITIVE.SendTo(src);
                } else {
                    amt = amountArg.ToUshort;
                    goto give;
                }
                return;
            }

            give:
            var asset = optAsset.Value;
            var playersToReceive = new List<UPlayer>();
            var item = new Item(asset.id, true);

            if (asset is ItemFuelAsset) {
                item.Metadata[0] = 1;
            }

            if (!src.HasPermission("essentials.bypass.itemlimit") && amt > UEssentials.Config.ItemSpawnLimit) {
                amt = UEssentials.Config.ItemSpawnLimit;
                EssLang.ITEM_LIMIT.SendTo(src, amt);
            }

            if (allPlayers) {
                UServer.Players.ForEach(playersToReceive.Add);
                EssLang.GIVEN_ITEM_ALL.SendTo(src, amt, asset.Name, asset.Id);
            } else {
                playersToReceive.Add(target);

                if (!src.IsConsole && src.ToPlayer() == target) {
                    goto give2;
                }

                EssLang.GIVEN_ITEM.SendTo(src, amt, asset.Name, asset.Id, target.CharacterName);
            }

            give2:
            playersToReceive.ForEach(p => {
                var success = p.GiveItem(item, amt, true);

                EssLang.RECEIVED_ITEM.SendTo(p, amt, asset.Name, asset.Id);

                if (!success) {
                    EssLang.INVENTORY_FULL.SendTo(p);
                }
            });
        }

        #endregion
    }

}