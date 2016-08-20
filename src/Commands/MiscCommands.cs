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
        internal static readonly HashSet<ulong> Spies = new HashSet<ulong>();

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
                return CommandResult.Lang("INVALID_NUMBER", args[0]);
            }

            if (args[0].ToFloat <= 0) {
                return CommandResult.Lang("MUST_POSITIVE", args[0]);
            }

            var player = src.ToPlayer();
            var pos = new Vector3(player.Position.x, player.Position.y, player.Position.z);
            var num = args[0].ToFloat;

            pos.y += num;

            player.Teleport(pos);
            EssLang.Send(src, "ASCENDED", num);

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
                return CommandResult.Lang("INVALID_NUMBER", args[0]);
            }

            if (args[0].ToFloat <= 0) {
                return CommandResult.Lang("MUST_POSITIVE", args[0]);
            }

            var player = src.ToPlayer();
            var pos = new Vector3(player.Position.x, player.Position.y, player.Position.z);
            var num = args[0].ToFloat;

            pos.y -= num;

            player.Teleport(pos);
            EssLang.Send(src, "DESCENDED", num);

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

            //var distance = -1;

            if (args.Length > 1) {
                if (src.IsConsole) {
                    return CommandResult.ShowUsage();
                }

                if (!args[1].IsInt) {
                    return CommandResult.Lang("INVALID_NUMBER", args[1]);
                }

                if (args[1].ToInt < 1) {
                    return CommandResult.Lang("NUMBER_BETWEEN", 1, int.MaxValue);
                }

                //distance = args[1].ToInt;
            }

            switch (args[0].ToLowerString) {
                case "ev":
                case "emptyvehicles":
                    return CommandResult.Generic("This option is currently disabled.");
                    /*if (!src.HasPermission(cmd.Permission + ".emptyvehicles")) {
                        return CommandResult.Lang("COMMAND_NO_PERMISSION");
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

                        EssLang.Send(src, "CLEAR_EMPTY_VEHICLES", toRemove.Count);
                    }).Start();
                    break;*/

                case "v":
                case "vehicle":
                    return CommandResult.Generic("This option is currently disabled.");
                    /*
                    if (!src.HasPermission(cmd.Permission + ".vehicles")) {
                        return CommandResult.Lang("COMMAND_NO_PERMISSION");
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
                        EssLang.Send(src, "CLEAR_VEHICLES");
                    }).Start();
                    break;*/

                case "i":
                case "items":
                    if (!src.HasPermission(cmd.Permission + ".items")) {
                        return CommandResult.Lang("COMMAND_NO_PERMISSION");
                    }

                    ItemManager.askClearAllItems();
                    EssLang.Send(src, "CLEAR_ITEMS");
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
                    } else if (args[0].Equals("*")) {
                        GiveItem(src, null, args[1], One, true);
                    } else if (!args[0].IsValidPlayerIdentifier) {
                        return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                    } else {
                        GiveItem(src, args[0].ToPlayer, args[1], One);
                    }
                    break;

                /*
                    /i [player] [item] [amount]
                    /i all [item] [amount]
                 */
                case 3:
                    if (args[0].Equals("*")) {
                        GiveItem(src, null, args[1], args[2], true);
                    } else if (!args[0].IsValidPlayerIdentifier) {
                        return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                    } else {
                        GiveItem(src, args[0].ToPlayer, args[1], args[2]);
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
            Usage = "<item id>"
        )]
        private CommandResult ItemInfoCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            if (src.IsConsole && args.Length != 1) {
                return CommandResult.ShowUsage();
            }

            ItemAsset asset;

            if (args.Length == 0) {
                var equipment = src.ToPlayer().Equipment;

                if (equipment.HoldingItemID == 0) {
                    return CommandResult.Lang("EMPTY_HANDS");
                }

                asset = equipment.asset;
            } else {
                if (!args[0].IsUShort ||
                    (asset = Assets.find(EAssetType.ITEM, args[0].ToUShort) as ItemAsset) == null) {
                    return CommandResult.Lang("INVALID_ITEM_ID", args[0]);
                }
            }

            var color = Color.yellow;
            var name = WrapMessage(src, asset.Name);
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
            Usage = "[autoreload | autorepair | all] [on | off]",
            Description = "Item features",
            AllowedSource = AllowedSource.PLAYER,
            MinArgs = 2,
            MaxArgs = 2
        )]
        private CommandResult ItemFeaturesCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            var toggleVal = GetToggleValue(args[1]);

            if (!toggleVal.HasValue) {
                return CommandResult.Lang("INVALID_BOOLEAN", args[1]);
            }

            var player = src.ToPlayer();
            var component = player.GetComponent<ItemFeatures>() ?? player.AddComponent<ItemFeatures>();

            switch (args[0].ToLowerString) {
                case "autoreload":
                    if (!src.HasPermission($"{cmd.Permission}.autoreload")) {
                        return CommandResult.NoPermission($"{cmd.Permission}.autoreload");
                    }
                    if (toggleVal.Value) {
                        component.AutoReload = true;
                        EssLang.Send(src, "AUTO_RELOAD_ENABLED");
                    } else {
                        component.AutoReload = false;
                        EssLang.Send(src, "AUTO_RELOAD_DISABLED");
                    }
                    break;

                case "autorepair":
                    if (!src.HasPermission($"{cmd.Permission}.autorepair")) {
                        return CommandResult.NoPermission($"{cmd.Permission}.autorepair");
                    }
                    if (toggleVal.Value) {
                        component.AutoRepair = true;
                        EssLang.Send(src, "AUTO_REPAIR_ENABLED");
                    } else {
                        component.AutoRepair = false;
                        EssLang.Send(src, "AUTO_REPAIR_DISABLED");
                    }
                    break;

                case "all":
                    if (!src.HasPermission($"{cmd.Permission}.all")) {
                        return CommandResult.NoPermission($"{cmd.Permission}.all");
                    }
                    if (toggleVal.Value) {
                        component.AutoReload = true;
                        component.AutoRepair = true;
                        EssLang.Send(src, "AUTO_RELOAD_ENABLED");
                        EssLang.Send(src, "AUTO_REPAIR_ENABLED");
                    } else {
                        component.AutoReload = false;
                        component.AutoRepair = false;
                        EssLang.Send(src, "AUTO_RELOAD_DISABLED");
                        EssLang.Send(src, "AUTO_REPAIR_DISABLED");
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
            Usage = "[autorefuel | autorepair | all] [on|off]",
            Description = "Vehicle features",
            AllowedSource = AllowedSource.PLAYER,
            MinArgs = 2,
            MaxArgs = 2
        )]
        private CommandResult VehicleFeaturesCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            var toggleVal = GetToggleValue(args[1]);

            if (!toggleVal.HasValue) {
                return CommandResult.Lang("INVALID_BOOLEAN", args[1]);
            }

            var player = src.ToPlayer();
            var component = player.GetComponent<VehicleFeatures>() ?? player.AddComponent<VehicleFeatures>();

            switch (args[0].ToLowerString) {
                case "autorefuel":
                    if (!src.HasPermission($"{cmd.Permission}.autorefuel")) {
                        return CommandResult.NoPermission($"{cmd.Permission}.autorefuel");
                    }
                    if (toggleVal.Value) {
                        component.AutoRefuel = true;
                        EssLang.Send(src, "AUTO_REFUEL_ENABLED");
                    } else {
                        component.AutoRefuel = false;
                        EssLang.Send(src, "AUTO_REFUEL_DISABLED");
                    }
                    break;

                case "autorepair":
                    if (!src.HasPermission($"{cmd.Permission}.autorepair")) {
                        return CommandResult.NoPermission($"{cmd.Permission}.autorepair");
                    }
                    if (toggleVal.Value) {
                        component.AutoRepair = true;
                        EssLang.Send(src, "AUTO_REPAIR_ENABLED");
                    } else {
                        component.AutoRepair = false;
                        EssLang.Send(src, "AUTO_REPAIR_DISABLED");
                    }
                    break;

                case "all":
                    if (!src.HasPermission($"{cmd.Permission}.all")) {
                        return CommandResult.NoPermission($"{cmd.Permission}.all");
                    }
                    if (toggleVal.Value) {
                        component.AutoRepair = true;
                        component.AutoRefuel = true;
                        EssLang.Send(src, "AUTO_REPAIR_ENABLED");
                        EssLang.Send(src, "AUTO_REFUEL_ENABLED");
                    } else {
                        component.AutoRepair = false;
                        component.AutoRefuel = false;
                        EssLang.Send(src, "AUTO_REPAIR_DISABLED");
                        EssLang.Send(src, "AUTO_REFUEL_DISABLED");
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
                EssLang.Send(src, "SPY_MODE_OFF");
            } else {
                Spies.Add(playerId);
                EssLang.Send(src, "SPY_MODE_ON");
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
                }
                var player = src.ToPlayer();

                EssLang.Send(player,
                    "POSITION",
                    player.Position.x,
                    player.Position.y,
                    player.Position.z);
            } else {
                var found = UPlayer.TryGet(args[0], p => {
                    EssLang.Send(src, "POSITION_OTHER", p.DisplayName, p.Position.x, p.Position.y, p.Position.z);
                });

                if (!found) {
                    return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                }
            }

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "online",
            Description = "View the number of online players"
        )]
        private CommandResult OnlineCommand(ICommandSource src, ICommandArgs args, ICommand cmd) {
            EssLang.Send(src, "ONLINE_PLAYERS", UServer.Players.Count(), UServer.MaxPlayers);

            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "respawnitems",
            Description = "Respawn all items."
        )]
        private CommandResult RespawnItemsCommand(ICommandSource src, ICommandArgs args) {
            for (byte x = 0; x < Regions.WORLD_SIZE; x++) {
                for (byte y = 0; y < Regions.WORLD_SIZE; y++) {

                    var itemsCount = LevelItems.spawns[x, y].Count;
                    if (itemsCount <= 0) continue;

                    for (var i = 0; i < itemsCount; i++) {
                        var itemSpawnpoint = LevelItems.spawns[x, y][i];
                        var itemId = LevelItems.getItem(itemSpawnpoint);

                        if (itemId == 0) continue;

                        var item = new Item(itemId, true);
                        ItemManager.dropItem(item, itemSpawnpoint.point, false, false, false);
                    }
                }
            }
            EssLang.Send(src, "RESPAWNED_ITEMS");
            return CommandResult.Success();
        }


        [CommandInfo(
            Name = "respawnvehicles",
            Description = "Respawn all vehicles."
        )]
        private CommandResult RespawnVehiclesCommand(ICommandSource src, ICommandArgs args) {
            var spawns = LevelVehicles.spawns;

            foreach (var vehicleSpawnpoint in spawns) {
                var vehicleId = LevelVehicles.getVehicle(vehicleSpawnpoint);

                if (vehicleId == 0) continue;

                var point = vehicleSpawnpoint.point;
                point.y += 1f;
                VehicleManager.spawnVehicle(vehicleId, point, Quaternion.Euler(0f, vehicleSpawnpoint.angle, 0f));
            }

            EssLang.Send(src, "RESPAWNED_VEHICLES");

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
                        return CommandResult.Lang("INVALID_VEHICLE_ID", args[0]);
                    }

                    var id = optAsset.Value.id;

                    if (UEssentials.Config.VehicleBlacklist.Contains(id) &&
                        !src.HasPermission("essentials.bypass.blacklist.vehicle")) {
                        return CommandResult.Lang("BLACKLISTED_VEHICLE", $"{optAsset.Value.vehicleName} ({id})");
                    }

                    VehicleTool.giveVehicle(src.ToPlayer().UnturnedPlayer, id);

                    EssLang.Send(src, "RECEIVED_VEHICLE", optAsset.Value.Name, id);
                    break;

                case 2:
                    if (!src.HasPermission($"{cmd.Permission}.other")) {
                        return CommandResult.Lang("COMMAND_NO_PERMISSION");
                    }

                    optAsset = VehicleUtil.GetVehicle(args[1].ToString());

                    if (optAsset.IsAbsent) {
                        return CommandResult.Lang("INVALID_VEHICLE_ID", args[1]);
                    }

                    var vehAsset = optAsset.Value;

                    if (args[0].Equals("*")) {
                        UServer.Players.ForEach(p => {
                            VehicleTool.giveVehicle(p.UnturnedPlayer, vehAsset.id);
                        });

                        EssLang.Send(src, "GIVEN_VEHICLE_ALL", vehAsset.Name, vehAsset.Id);
                    } else if (!args[0].IsValidPlayerIdentifier) {
                        return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                    } else {
                        var target = args[0].ToPlayer;
                        VehicleTool.giveVehicle(target.UnturnedPlayer, vehAsset.id);

                        EssLang.Send(src, "GIVEN_VEHICLE", vehAsset.Name, vehAsset.Id, target.DisplayName);
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
            var tps = Provider.debugTPS;
            var color =
                tps > 40 ? Color.green :
                tps > 25 ? Color.yellow :
                Color.red;

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
                        return CommandResult.Lang("INVALID_SKILL", args[0]);
                    }

                    var player = src.ToPlayer();
                    byte value;

                    if (args[1].Equals("max")) {
                        value = player.GetSkill(optSkill.Value).max;
                    } else if (args[1].IsInt) {
                        if (args[1].ToInt < 0 || args[1].ToInt > byte.MaxValue) {
                            return CommandResult.Lang("NEGATIVE_OR_LARGE");
                        }

                        value = (byte) args[1].ToInt;
                    } else {
                        return CommandResult.Lang("INVALID_NUMBER", args[1]);
                    }

                    player.SetSkillLevel(optSkill.Value, value);
                    EssLang.Send(src, "SKILL_SET", optSkill.Value.Name.Capitalize(), args[1]);
                    break;

                // /skill [player|*] [skill] [value]
                case 3:
                    if (args[0].Equals("*")) {
                        if (!UServer.Players.Any()) {
                            return CommandResult.Lang("ANYONE_ONLINE");
                        }

                        player = UServer.Players.First();
                    } else {
                        if (!args[0].IsValidPlayerIdentifier) {
                            return CommandResult.Lang("PLAYER_NOT_FOUND");
                        }

                        player = args[0].ToPlayer;
                    }

                    optSkill = USkill.FromName(args[1].ToString());

                    if (optSkill.IsAbsent) {
                        return CommandResult.Lang("INVALID_SKILL", args[0]);
                    }

                    if (args[2].Equals("max")) {
                        value = player.GetSkill(optSkill.Value).max;
                    } else if (args[2].IsInt) {
                        if (args[2].ToInt < 0 || args[2].ToInt > byte.MaxValue) {
                            return CommandResult.Lang("NEGATIVE_OR_LARGE");
                        }

                        value = (byte) args[2].ToInt;
                    } else {
                        return CommandResult.Lang("INVALID_NUMBER", args[2]);
                    }

                    if (args[0].Equals("*")) {
                        UServer.Players.ForEach(p => p.SetSkillLevel(optSkill.Value, value));
                        EssLang.Send(src, "SKILL_SET_ALL", optSkill.Value.Name.Capitalize(), args[2]);
                    } else {
                        player.SetSkillLevel(optSkill.Value, value);
                        EssLang.Send(src, "SKILL_SET_PLAYER", optSkill.Value.Name.Capitalize(),
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
            Description = "Enable or disable server pvp.",
            Usage = "[on|off]"
        )]
        private CommandResult PvpCommand(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty) {
                return CommandResult.ShowUsage();
            }

            var toggleVal = GetToggleValue(args[0]);

            if (!toggleVal.HasValue) {
                return CommandResult.ShowUsage();
            }
            if (toggleVal.Value) {
                Provider.PvP = true;
                EssLang.Send(src, "PVP_ENABLED");
            } else {
                Provider.PvP = false;
                EssLang.Send(src, "PVP_DISABLED");
            }
            return CommandResult.Success();
        }

#if DEV
        [CommandInfo(
            Name = "stoptasks"
        )]
        private CommandResult StopTasksCommand(ICommandSource src, ICommandArgs args) {
            UEssentials.TaskExecutor.DequeueAll();
            return CommandResult.Success();
        }

        [CommandInfo(
            Name = "cls"
        )]
        private CommandResult ClsCommand(ICommandSource src, ICommandArgs args) {
            Console.Clear();
            return CommandResult.Success();
        }
#endif

        #region HELPER METHODS

        private static bool? GetToggleValue(ICommandArgument arg) {
            switch (arg.RawValue.ToLowerInvariant()) {
                case "true":
                case "on":
                case "1":
                    return true;

                case "false":
                case "off":
                case "0":
                    return false;

                default:
                    return null;
            }
        }

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
                EssLang.Send(src, "COMMAND_NO_PERMISSION");
                return;
            }

            var optAsset = ItemUtil.GetItem(itemArg.ToString());

            if (optAsset.IsAbsent) {
                EssLang.Send(src, "ITEM_NOT_FOUND", itemArg);
                return;
            }

            if (UEssentials.Config.GiveItemBlacklist.Contains(optAsset.Value.id) &&
                !src.HasPermission("essentials.bypass.blacklist.item")) {
                EssLang.Send(src, "BLACKLISTED_ITEM", $"{optAsset.Value.itemName} ({optAsset.Value.Id})");
                return;
            }

            ushort amt = 1;

            if (amountArg != null) {
                if (!amountArg.IsShort) {
                    EssLang.Send(src, "INVALID_NUMBER", amountArg);
                } else if (amountArg.ToShort <= 0) {
                    EssLang.Send(src, "MUST_POSITIVE");
                } else {
                    amt = amountArg.ToUShort;
                    goto give;
                }
                return;
            }

            give:
            var asset = optAsset.Value;
            var playersToReceive = new List<UPlayer>();
            var item = new Item(asset.id, true);

            if (asset is ItemFuelAsset) {
                item.Metadata[0] = 244;
                item.Metadata[1] = 1;
            }

            if (!src.HasPermission("essentials.bypass.itemlimit") && amt > UEssentials.Config.ItemSpawnLimit) {
                amt = UEssentials.Config.ItemSpawnLimit;
                EssLang.Send(src, "ITEM_LIMIT", amt);
            }

            if (allPlayers) {
                UServer.Players.ForEach(playersToReceive.Add);
                EssLang.Send(src, "GIVEN_ITEM_ALL", amt, asset.Name, asset.Id);
            } else {
                playersToReceive.Add(target);

                if (!src.IsConsole && src.ToPlayer() == target) {
                    goto give2;
                }

                EssLang.Send(src, "GIVEN_ITEM", amt, asset.Name, asset.Id, target.CharacterName);
            }

            give2:
            playersToReceive.ForEach(p => {
                var success = p.GiveItem(item, amt, true);

                EssLang.Send(p, "RECEIVED_ITEM", amt, asset.Name, asset.Id);

                if (!success) {
                    EssLang.Send(p, "INVENTORY_FULL");
                }
            });
        }

        #endregion
    }

}