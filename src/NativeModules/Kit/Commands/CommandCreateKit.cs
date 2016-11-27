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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using Essentials.NativeModules.Kit.Item;
using static Essentials.Common.Util.ItemUtil;
using SDG.Unturned;

namespace Essentials.NativeModules.Kit.Commands {

    [CommandInfo(
        Name = "createkit",
        Aliases = new[] { "ckit" },
        Description = "Create a kit using your inventory items.",
        Usage = "[name] <cooldown> <resetCooldownWhenDie> <cost>",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandCreateKit : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();

            if (args.Length < 1) {
                return CommandResult.ShowUsage();
            }

            var name = args[0].ToString();
            uint cooldown = 0;
            var resetCooldownWhenDie = false;
            var cost = 0d;

            if (KitModule.Instance.KitManager.Contains(name)) {
                return CommandResult.Lang("KIT_ALREADY_EXIST", name);
            }

            if (args.Length > 1) {
                if (!args[1].IsInt) {
                    return CommandResult.Lang("INVALID_NUMBER", args[1]);
                }

                if (args[1].ToInt < 0) {
                    return CommandResult.Lang("MUST_POSITIVE");
                }

                cooldown = args[1].ToUInt;
            }

            if (args.Length > 2) {
                if (!args[2].IsBool) {
                    return CommandResult.Lang("INVALID_BOOLEAN", args[2]);
                }

                resetCooldownWhenDie = args[2].ToBool;
            }

            if (args.Length > 3) {
                if (!args[3].IsDouble) {
                    return CommandResult.Lang("INVALID_NUMBER", args[3]);
                }

                if (args[3].ToDouble < 0) {
                    return CommandResult.Lang("MUST_POSITIVE");
                }

                cost = args[3].ToDouble;
            }

            var inventory = player.Inventory;
            var clothing = player.Clothing;
            var items = new List<AbstractKitItem>();

            Action<byte> addItemsFromPage = page => {
                var count = inventory.getItemCount(page);

                for (byte index = 0; index < count; index++) {
                    var item = inventory.getItem(page, index).item;

                    if (item == null) {
                        continue;
                    }

                    var asset = GetItem(item.id).Value;
                    KitItem kitItem;

                    if (asset is ItemWeaponAsset) {
                        var ammo = GetWeaponAmmo(item);
                        var firemode = GetWeaponFiremode(item).OrElse(EFiremode.SAFETY);

                        var kItem = new KitItemWeapon(item.id, item.durability, 1, ammo.OrElse(0), firemode) {
                            Magazine = GetWeaponAttachment(item, AttachmentType.MAGAZINE).OrElse(null),
                            Barrel = GetWeaponAttachment(item, AttachmentType.BARREL).OrElse(null),
                            Sight = GetWeaponAttachment(item, AttachmentType.SIGHT).OrElse(null),
                            Grip = GetWeaponAttachment(item, AttachmentType.GRIP).OrElse(null),
                            Tactical = GetWeaponAttachment(item, AttachmentType.TACTICAL).OrElse(null)
                        };

                        kitItem = kItem;
                    } else if (asset is ItemMagazineAsset || asset is ItemSupplyAsset) {
                        kitItem = new KitItemMagazine(item.id, item.durability, 1, item.amount);
                    } else {
                        kitItem = new KitItem(item.id, item.durability, 1);
                    }

                    kitItem.Metadata = item.metadata;

                    items.Add(kitItem);
                }
            };

            addItemsFromPage(0); // Primary slot
            addItemsFromPage(1); // Secondary slot
            addItemsFromPage(2); // Hands

            // Backpack

            if (clothing.backpack != 0) {
                items.Add(new KitItem(clothing.backpack, clothing.backpackQuality, 1) {
                    Metadata = clothing.backpackState
                });
            }

            addItemsFromPage(3);

            // End Backpack

            // Shirt

            if (clothing.shirt != 0) {
                items.Add(new KitItem(clothing.shirt, clothing.shirtQuality, 1) {
                    Metadata = clothing.shirtState
                });
            }

            addItemsFromPage(5);

            // End Shirt

            // Vest

            if (clothing.vest != 0) {
                items.Add(new KitItem(clothing.vest, clothing.vestQuality, 1) {
                    Metadata = clothing.vestState
                });
            }

            addItemsFromPage(4);

            // End Vest

            // Pants

            if (clothing.pants != 0) {
                items.Add(new KitItem(clothing.pants, clothing.pantsQuality, 1) {
                    Metadata = clothing.pantsState
                });
            }

            addItemsFromPage(6);

            // End Pants

            // Mask, Glasses & Hat

            if (clothing.mask != 0) {
                items.Add(new KitItem(clothing.mask, clothing.maskQuality, 1) {
                    Metadata = clothing.maskState
                });
            }

            if (clothing.hat != 0) {
                items.Add(new KitItem(clothing.hat, clothing.hatQuality, 1) {
                    Metadata = clothing.hatState
                });
            }

            if (clothing.glasses != 0) {
                items.Add(new KitItem(clothing.glasses, clothing.glassesQuality, 1) {
                    Metadata = clothing.glassesState
                });
            }

            // End Mask, Glasses & Hat

            var kit = new Kit(name, cooldown, (decimal) cost, resetCooldownWhenDie) {
                Items = items
            };

            KitModule.Instance.KitManager.Add(kit);
            EssLang.Send(src, "CREATED_KIT", name);

            return CommandResult.Success();
        }

    }

}