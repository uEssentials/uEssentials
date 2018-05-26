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
        Description = "Edit a kit",
        Usage = "[kit] [view | additem | delitem | set]"
    )]
    public class CommandEditKit : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.Length < 2) {
                src.SendMessage(UsageMessage);
                src.SendMessage("SubCommands' Usage:");
                src.SendMessage(" - additem [type(normal|vehicle|xp)] ...");
                src.SendMessage("  * To add a vehicle: additem vehicle [id]");
                src.SendMessage("  * To add a item: additem normal [id] [amount] [durability]");
                src.SendMessage("  * To add a xp: additem xp [amount]");
                src.SendMessage("  * To add a money: additem money [amount]");
                src.SendMessage(" - delitem [itemIndex]");
                src.SendMessage("  * Use '/ekit [kit] view' to list valid indexes.");
                src.SendMessage(" - set [option] [value]");
                src.SendMessage("  * Valid options:");
                src.SendMessage("   - nm = Name | Accepts a string");
                src.SendMessage("   - cst = Cost | Accepts a Number");
                src.SendMessage("   - cd  = Cooldown | Accepts a Number");
                src.SendMessage("   - rwd = ResetCooldownWhenDie | Accepts a Boolean (true or false)");
                return CommandResult.InvalidArgs();
            }

            var kitManager = KitModule.Instance.KitManager;
            var kitName = args[0].ToString();

            if (!kitManager.Contains(kitName)) {
                return CommandResult.LangError("KIT_NOT_EXIST", kitName);
            }

            var kit = kitManager.GetByName(kitName);

			switch (args[1].ToLowerString) {
                case "view":
                    src.SendMessage($"Name: {kit.Name}");
                    src.SendMessage($"Cooldown: {kit.Cooldown}");
                    src.SendMessage($"Cost: {kit.Cost}");
                    src.SendMessage($"ResetCooldownWhenDie: {kit.ResetCooldownWhenDie}");
                    src.SendMessage(string.Empty);

                    if (kit.Items.Count > 0) {
                        src.SendMessage("Items:");

                        var index = 0;
                        kit.Items.ForEach(item => src.SendMessage(item.ToString().Insert(0, $" [{(++index)}] ")));
                    } else {
                        src.SendMessage("This kit has no items.");
                    }
                    return CommandResult.Success();

                /* ekit name additem normal id amount durability */
                case "additem":
                    if (args.Length < 3) {
                        return CommandResult.InvalidArgs("Use /ekit [kit] additem [type] [id] [amount] [durability]");
                    }

                    byte? durability = null;
                    byte? amount = null;

                    // Try to convert 'amount'
                    if (args.Length >= 5 && args[4].TryConvertToByte(out amount, out var error)) {
                        return error;
                    }

                    // Try to convert 'durability'
                    if (args.Length >= 6 && args[5].TryConvertToByte(out durability, out error)) {
                        return error;
                    }

                    switch (args[2].ToLowerString) {
                        case "normal":
                            if (args.Length < 4) {
                                return CommandResult.InvalidArgs("Use /ekit [kit] additem normal [id] [amount] [durability]");
                            }

                            var optAsset = ItemUtil.GetItem(args[3].ToString());

                            if (optAsset.IsAbsent) {
                                return CommandResult.LangError("INVALID_ITEM_ID_NAME", args[3]);
                            }

                            kit.Items.Add(new KitItem(optAsset.Value.id, durability ?? 100, amount ?? 1));
                            src.SendMessage($"Added Id: {optAsset.Value.id}, Amount: {amount}, " +
                                            $"Durability: {durability}");
                            break;

                        case "vehicle":
                            if (args.Length != 4) {
                                return CommandResult.InvalidArgs("Use /ekit [kit] additem vehicle [id]");
                            }

                            if (!args[3].IsInt) {
                                return CommandResult.LangError("INVALID_NUMBER", args[3]);
                            }

                            var argAsInt = args[3].ToInt;

                            if (argAsInt < 0 || argAsInt > ushort.MaxValue) {
                                return CommandResult.LangError("NEGATIVE_OR_LARGE");
                            }

                            var vehicleAsset = Assets.find(EAssetType.VEHICLE, (ushort) argAsInt);

                            if (vehicleAsset == null) {
                                return CommandResult.LangError("INVALID_VEHICLE_ID", argAsInt);
                            }

                            kit.Items.Add(new KitItemVehicle((ushort) argAsInt));
                            src.SendMessage($"Added Vehicle item. Id: {argAsInt}");
                            break;

                        case "money":
                            if (args.Length != 4) {
                                return CommandResult.InvalidArgs("Use /ekit [kit] additem money [amount]");
                            }

                            if (!args[3].IsInt) {
                                return CommandResult.LangError("INVALID_NUMBER", args[3]);
                            }

                            if (args[3].ToInt < 0) {
                                return CommandResult.LangError("MUST_POSITIVE");
                            }

                            var moneyAmount = args[3].ToUInt;
                            kit.Items.Add(new KitItemMoney(moneyAmount));
                            src.SendMessage($"Added money item. Amount: {moneyAmount}");
                            break;

                        case "xp":
                            if (args.Length != 4) {
                                return CommandResult.InvalidArgs("Use /ekit [kit] additem xp [amount]");
                            }

                            if (!args[3].IsInt) {
                                return CommandResult.LangError("INVALID_NUMBER", args[3]);
                            }

                            if (args[3].ToInt < 0) {
                                return CommandResult.LangError("MUST_POSITIVE");
                            }

                            var expAmount = args[3].ToUInt;

                            kit.Items.Add(new KitItemExperience(expAmount));
                            src.SendMessage($"Added Xp item. Experience Amount: {expAmount}");
                            break;

                        default:
                            return CommandResult.Error($"Invalid type '{args[2]}'. Valid types are: 'normal', 'money', 'vehicle' and 'xp'");
                    }
                    break;

                /* ekit name delitem [itemindex] */
                case "delitem":
                    if (args.Length != 3) {
                        src.SendMessage("Use '/ekit [kit] delitem [itemIndex]'");
                        src.SendMessage("Use '/ekit [kit] view' to view valid indexes.");

                        return CommandResult.InvalidArgs();
                    }

                    if (!args[2].IsInt) {
                        return CommandResult.LangError("INVALID_NUMBER", args[2]);
                    }

                    var argAsInt2 = args[2].ToInt;

                    if (argAsInt2 <= 0) {
                        return CommandResult.LangError("MUST_POSITIVE");
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
                        src.SendMessage("cst = Cost");
                        src.SendMessage("cd  = Cooldown");
                        src.SendMessage("rwd = ResetCooldownWhenDie");

                        return CommandResult.InvalidArgs();
                    }

                    switch (args[2].ToLowerString) {
                        case "cost":
                        case "cst":
                            if (!args[3].IsDouble) {
	                            return CommandResult.LangError("INVALID_NUMBER", args[3]);
                            }

                            kit.Cost = (decimal) args[3].ToDouble;
                            src.SendMessage("Cost set to " + kit.Cost);
                            break;

                        case "name":
                        case "nm":
                            kit.Name = args[3].ToString();
                            src.SendMessage("Name set to " + kit.Name);
                            break;

                        case "cooldown":
                        case "cd":
                            if (!args[3].IsInt) {
                                return CommandResult.LangError("INVALID_NUMBER", args[3]);
                            }

                            if (args[3].ToInt < 0) {
                                return CommandResult.LangError("MUST_POSITIVE");
                            }

                            kit.Cooldown = args[3].ToUInt;
                            src.SendMessage("Cooldown set to " + kit.Cooldown);
                            break;

                        case "resetcooldownwhendie":
                        case "rwd":
                            if (!args[3].IsBool) {
                                return CommandResult.LangError("INVALID_BOOLEAN", args[3]);
                            }

                            kit.ResetCooldownWhenDie = args[3].ToBool;
                            src.SendMessage("ResetCooldownWhenDie set to " + kit.ResetCooldownWhenDie);
                            break;

                        default:
                            src.SendMessage("nm  = Name");
                            src.SendMessage("cd  = Cooldown");
                            src.SendMessage("cst = Cost");
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