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
using System.IO;
using Essentials.Api;
using Essentials.Common.Util;
using Essentials.Core.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Items;
using SDG.Unturned;
using Essentials.Core;
using Essentials.Compatibility.Hooks;
using Essentials.NativeModules.Kit.Item;

namespace Essentials.NativeModules.Kit.Data {

    internal class KitData : IData<Dictionary<string, Kit>> {

        protected static string DataFilePath => $"{UEssentials.PluginFolder}kits.json";

        private const StringComparison _cmpIgnoreCase = StringComparison.InvariantCultureIgnoreCase;

        public virtual void Save(Dictionary<string, Kit> warps) {
            File.WriteAllText(DataFilePath, string.Empty);
            JsonUtil.Serialize(DataFilePath, warps.Values);
        }

        private static T GetValueOrDefault<T>(JObject obj, string key, T def) {
            var val = obj.GetValue(key).Value<T>();
            if (val == null) {
                return def;
            }
            return val;
        }

        private static T GetNotNullValue<T>(JObject obj, string key, string errMsg = null) {
            var val = obj.GetValue(key).Value<T>();
            if (val == null) {
                throw new ArgumentException(errMsg ?? $"Kit {key} cannot be null.");
            }
            return val;
        }

        public virtual Dictionary<string, Kit> Load() {
            var loadedKits = new Dictionary<string, Kit>();

            if (!File.Exists(DataFilePath)) {
                File.Create(DataFilePath).Close();
                LoadDefault();
            }

            JArray kitArr;

            try {
                kitArr = JArray.Parse(File.ReadAllText(DataFilePath));
            } catch (JsonReaderException ex) {
                UEssentials.Logger.LogError($"Invalid kit configuration ({DataFilePath})");
                UEssentials.Logger.LogError(ex.ToString());
                kitArr = new JArray();
            }

            const StringComparison strCmp = StringComparison.InvariantCultureIgnoreCase;

            foreach (var kitObj in kitArr.Children<JObject>()) {
                var kit = new Kit(
                    GetNotNullValue<string>(kitObj, "Name"),
                    GetValueOrDefault(kitObj, "Cooldown", 0u),
                    GetValueOrDefault(kitObj, "Cost", 0m),
                    GetValueOrDefault(kitObj, "ResetCooldownWhenDie", false)
                );

                var itemIndex = 0;
                var economyHook = EssCore.Instance.HookManager.GetActiveByType<UconomyHook>();

                foreach (var itemObj in kitObj.GetValue("Items", strCmp).Children<JObject>()) {
                    AbstractKitItem kitItem;
                    JToken val;

                    if ((val = itemObj.GetValue("money", strCmp)) != null && economyHook.IsPresent) {
                        kitItem = new KitItemMoney(val.Value<decimal>());
                        goto add;
                    }

                    if ((val = itemObj.GetValue("xp", strCmp)) != null) {
                        kitItem = new KitItemExperience(val.Value<uint>());
                        goto add;
                    }

                    if ((val = itemObj.GetValue("vehicle", strCmp)) != null) {
                        var vehicleId = val.Value<ushort>();

                        if (Assets.find(EAssetType.VEHICLE, vehicleId) == null) {
                            UEssentials.Logger.LogWarning(
                                $"Invalid vehicle id '{vehicleId}' in kit '{kit.Name}' at index {++itemIndex}");
                            continue;
                        }

                        kitItem = new KitItemVehicle(vehicleId);
                        goto add;
                    }

                    var kitItemId = itemObj.GetValue("id", strCmp).Value<ushort>();
                    var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, kitItemId);

                    if (itemAsset == null) {
                        UEssentials.Logger.LogWarning($"Invalid item id '{kitItemId}' in kit '{kit.Name}' at index {++itemIndex}");
                        continue;
                    }

                    var tokKitItemDurability = itemObj.GetValue("Durability", strCmp);
                    var tokKitItemAmount = itemObj.GetValue("Amount", strCmp);
                    var tokAmmo = itemObj.GetValue("Ammo", strCmp);

                    var kitItemAmount = tokKitItemAmount?.Value<byte>() ?? 1;
                    var kitItemDurability = tokKitItemDurability?.Value<byte>() ?? 100;

                    if (itemAsset.UseableType == EUseableType.GUN)
                        goto weaponItem;

                    if (itemAsset is ItemMagazineAsset || itemAsset is ItemSupplyAsset) {
                        byte magazineAmmo;
                        if (tokAmmo == null) {
                            magazineAmmo = ((ItemMagazineAsset) itemAsset).Amount;
                        } else {
                            magazineAmmo = tokAmmo.Value<byte>();
                        }
                        kitItem = new KitItemMagazine(kitItemId, kitItemDurability, kitItemAmount, magazineAmmo);
                    } else {
                        kitItem = new KitItem(kitItemId, kitItemDurability, kitItemAmount);

                        if (itemAsset is ItemFuelAsset) {
                            ((KitItem) kitItem).Metadata[0] = 244;
                            ((KitItem) kitItem).Metadata[1] = 1;
                        }
                    }
                    goto add;

                    weaponItem:
                    var tokFireMode = itemObj.GetValue("FireMode", strCmp);
                    var tokBarrel = itemObj.GetValue("Barrel", strCmp);
                    var tokSight = itemObj.GetValue("Sight", strCmp);
                    var tokGrip = itemObj.GetValue("Grip", strCmp);
                    var tokMagazine = itemObj.GetValue("Magazine", strCmp);
                    var tokTactical = itemObj.GetValue("Tatical", strCmp) ?? itemObj.GetValue("Tactical", strCmp);

                    EFiremode? fireMode = null;
                    var ammo = tokAmmo?.Value<byte>() ?? null;

                    if (tokFireMode != null) {
                        try {
                            fireMode = (EFiremode) Enum.Parse(typeof(EFiremode),
                                tokFireMode.Value<string>(), true);
                        } catch (ArgumentException) {
                            UEssentials.Logger.LogWarning($"Invalid firemode '{tokFireMode.Value<string>()}' " +
                                                          $"in kit '{kit.Name}', item '{itemIndex + 1}'!");
                        }
                    }

                    var weaponItem = new KitItemWeapon(kitItemId,
                        kitItemDurability,
                        kitItemAmount,
                        ammo,
                        fireMode
                    );

                    Func<JToken, Attachment> deserializeAttach = json => {
                        return json == null ? null : JsonConvert.DeserializeObject<Attachment>(json.ToString());
                    };

                    weaponItem.Barrel = deserializeAttach(tokBarrel);
                    weaponItem.Sight = deserializeAttach(tokSight);
                    weaponItem.Tactical = deserializeAttach(tokTactical);
                    weaponItem.Grip = deserializeAttach(tokGrip);
                    weaponItem.Magazine = deserializeAttach(tokMagazine);

                    kitItem = weaponItem;

                    add:
                    kit.Items.Add(kitItem);
                }
                loadedKits.Add(kit.Name.ToLowerInvariant(), kit);
            }

            return loadedKits;
        }

        private void LoadDefault() {
            var defaultKits = new Dictionary<string, Kit>();

            var defaultKit = new Kit("default", 120, true);
            var weaponKit = new Kit("default2", 1200, false);
            var planeKit = new Kit("plane", 9000, 100.0m, false);
            var xpKit = new Kit("xp", 1200, 50.0m, false);

            defaultKit.Items.Add(new KitItem(16, 100, 1));
            defaultKit.Items.Add(new KitItem(13, 100, 2));
            defaultKit.Items.Add(new KitItem(14, 100, 1));

            weaponKit.Items.Add(new KitItemWeapon(4, 100, 1, 30, EFiremode.BURST) {
                Barrel = new Attachment(7, 100),
                Grip = new Attachment(8, 100),
                Sight = new Attachment(146, 100),
                Magazine = new Attachment(17, 100),
                Tactical = new Attachment(151, 100)
            });

            planeKit.Items.Add(new KitItemVehicle(92));
            xpKit.Items.Add(new KitItemExperience(100));

            defaultKits.Add("default", defaultKit);
            defaultKits.Add("weapon", weaponKit);
            defaultKits.Add("plane", planeKit);
            defaultKits.Add("xp", xpKit);

            Save(defaultKits);
        }

    }

}