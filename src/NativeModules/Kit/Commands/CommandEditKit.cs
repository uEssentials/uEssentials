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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using Essentials.NativeModules.Kit.Item;
using SDG.Unturned;

// TODO: add translations

namespace Essentials.NativeModules.Kit.Commands {

    [CommandInfo(
        Name = "editkit",
        Aliases = new[] { "ekit" },
        Description = "Edit an kit",
        Usage = "[kit] [view | additem | delitem | set]"
    )]
    public class CommandEditKit : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.Length < 2) {
                return CommandResult.ShowUsage();
            }

            var kitManager = KitModule.Instance.KitManager;
            var kitName = args[0].ToString();

            if (!kitManager.Contains(kitName)) {
                return CommandResult.Lang("KIT_NOT_EXIST", kitName);
            }

            var kit = kitManager.GetByName(kitName);

            switch (args[1].ToLowerString) {
                case "view":
                    src.SendMessage($"Name: {kit.Name}");
                    src.SendMessage($"Cooldown: {kit.Cooldown}");
                    src.SendMessage($"ResetCooldownWhenDie: {kit.ResetCooldownWhenDie}");
                    src.SendMessage(string.Empty);
                    src.SendMessage("Items:");

                    var index = 0;

                    kit.Items.ForEach(i => {
                        src.SendMessage(i.ToString().Insert(0, $" [{(index++) + 1}] "));
                    });
                    break;

                // /ekit xp additem normal id amount durability
                case "additem":
                    if (args.Length < 3) {
                        return CommandResult.InvalidArgs("Use /ekit [kit] additem [type] [id] [amount] [durability]");
                    }

                    byte durability = 100;
                    byte amount = 1;

                    if (args.Length >= 5) {
                        if (!args[4].IsInt) {
                            return CommandResult.Lang("INVALID_NUMBER", args[4]);
                        }

                        var argAsInt = args[4].ToInt;

                        if (argAsInt < 0 || argAsInt > 255) {
                            return CommandResult.Lang("NEGATIVE_OR_LARGE");
                        }

                        amount = (byte) args[4].ToInt;
                    }

                    if (args.Length >= 6) {
                        if (!args[5].IsInt) {
                            return CommandResult.Lang("INVALID_NUMBER", args[5]);
                        }

                        var argAsInt = args[5].ToInt;

                        if (argAsInt < 0 || argAsInt > 255) {
                            return CommandResult.Lang("NEGATIVE_OR_LARGE");
                        }

                        durability = (byte) args[5].ToInt;
                    }

                    switch (args[2].ToLowerString) {
                        case "normal":
                            if (args.Length < 4) {
                                return
                                    CommandResult.InvalidArgs(
                                        "Use /ekit [kit] additem [type] [id] [amount] [durability]");
                            }

                            var optAsset = ItemUtil.GetItem(args[3].ToString());

                            if (optAsset.IsAbsent) {
                                return CommandResult.Lang("INVALID_ITEM_ID_NAME", args[3]);
                            }

                            kit.Items.Add(new KitItem(optAsset.Value.id, durability, amount));
                            src.SendMessage($"Added Id: {optAsset.Value.id}, Amount: {amount}, Durability: {durability}");
                            break;

                        case "vehicle":
                            if (args.Length != 4) {
                                return CommandResult.InvalidArgs("Use /ekit [kit] additem vehicle [id]");
                            }

                            if (!args[3].IsInt) {
                                return CommandResult.Lang("INVALID_NUMBER", args[3]);
                            }

                            var argAsInt = args[3].ToInt;

                            if (argAsInt < 0 || argAsInt > ushort.MaxValue) {
                                return CommandResult.Lang("NEGATIVE_OR_LARGE");
                            }

                            var vehicleAsset = Assets.find(EAssetType.VEHICLE, (ushort) argAsInt);

                            if (vehicleAsset == null) {
                                return CommandResult.Lang("INVALID_VEHICLE_ID", argAsInt);
                            }

                            kit.Items.Add(new KitItemVehicle((ushort) argAsInt));
                            src.SendMessage($"Added Vehicle item. Id: {argAsInt}");
                            break;

                        case "xp":
                            if (args.Length != 4) {
                                return CommandResult.InvalidArgs("Use /ekit [kit] additem xp [amount]");
                            }

                            if (!args[3].IsInt) {
                                return CommandResult.Lang("INVALID_NUMBER", args[3]);
                            }

                            if (args[3].ToInt < 0) {
                                return CommandResult.Lang("MUST_POSITIVE");
                            }

                            kit.Items.Add(new KitItemExperience(args[3].ToUint));
                            src.SendMessage($"Added Xp item. Amount: {amount}");
                            break;

                        default:
                            return
                                CommandResult.Error("Invalid type '{args[2]}'. Valid types are: normal, vehicle or xp.");
                    }
                    break;

                // /ekit kit delitem [itemindex]
                case "delitem":
                    if (args.Length != 3) {
                        src.SendMessage("Use '/ekit [kit] delitem [itemIndex]'");
                        src.SendMessage("Use '/ekit [kit] view' to view valid indexes.");

                        return CommandResult.InvalidArgs();
                    }

                    if (!args[2].IsInt) {
                        return CommandResult.Lang("INVALID_NUMBER", args[2]);
                    }

                    var argAsInt2 = args[2].ToInt;

                    if (argAsInt2 <= 0) {
                        return CommandResult.Lang("MUST_POSITIVE");
                    }

                    /* 1 to kitItems.Count */
                    if ((argAsInt2 - 1) > kit.Items.Count) {
                        src.SendMessage($"Invalid index, index must be between 1 and {kit.Items.Count}");
                        src.SendMessage("Use '/ekit [kit] view' to view valid indexes.");

                        return CommandResult.InvalidArgs();
                    }

                    kit.Items.RemoveAt(argAsInt2 - 1);
                    src.SendMessage($"Removed item at index {argAsInt2}");
                    break;

                case "set":
                    if (args.Length < 3) {
                        src.SendMessage("Use /ekit [kit] set [option] [value]");
                        src.SendMessage("nm  = Name");
                        src.SendMessage("cd  = Cooldown");
                        src.SendMessage("rwd = ResetCooldownWhenDie");

                        return CommandResult.InvalidArgs();
                    }

                    switch (args[2].ToLowerString) {
                        case "name":
                        case "nm":
                            kit.Name = args[3].ToString();
                            src.SendMessage("Name set to " + kit.Name);
                            break;

                        case "cooldown":
                        case "cd":
                            if (!args[3].IsInt) {
                                return CommandResult.Lang("INVALID_NUMBER", args[3]);
                            }

                            if (args[3].ToInt < 0) {
                                return CommandResult.Lang("MUST_POSITIVE");
                            }

                            kit.Cooldown = args[3].ToUint;
                            src.SendMessage("Cooldown set to " + kit.Cooldown);
                            break;

                        case "resetcooldownwhendie":
                        case "rwd":
                            if (!args[3].IsBool) {
                                return CommandResult.Lang("INVALID_BOOLEAN", args[3]);
                            }

                            kit.ResetCooldownWhenDie = args[3].ToBool;
                            src.SendMessage("ResetCooldownWhenDie set to " + kit.ResetCooldownWhenDie);
                            break;

                        default:
                            src.SendMessage("nm  = Name");
                            src.SendMessage("cd  = Cooldown");
                            src.SendMessage("rwd = ResetCooldownWhenDie");
                            return CommandResult.InvalidArgs();
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            kitManager.SaveKits();
            kitManager.LoadKits();

            return CommandResult.Success();
        }

    }

}