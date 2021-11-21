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
using Essentials.Common.Util;
using Essentials.Core.Storage;
using Essentials.NativeModules.Kit.Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Items;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Essentials.NativeModules.Kit.Data {

    internal class KitData : IData<Dictionary<string, Kit>> {



        public static string FixDeleteKits()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // windows
                return Rocket.Core.Environment.PluginsDirectory + "//uEssentials";
            }
            else
            {
                // linux
                return Rocket.Core.Environment.PluginsDirectory + "/uEssentials";
            }
        }


        internal static string DataFilePath => Path.Combine(FixDeleteKits(), "kits.json");

        public virtual void Save(Dictionary<string, Kit> warps) {
            JsonUtil.Serialize(DataFilePath, warps.Values);
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
                UEssentials.Logger.LogError($"Failed to parse kit configuration ({DataFilePath}). Invalid JSON!");
                UEssentials.Logger.LogException(ex);
                return new Dictionary<string, Kit>();
            }

            foreach (var kitObj in kitArr.Children<JObject>()) {
                T GetKitObjValueOrDefault<T>(string key) {
                    var val = kitObj.GetValue(key, StringComparison.InvariantCultureIgnoreCase);
                    return val == null ? default(T) : val.Value<T>();
                }

                var name = GetKitObjValueOrDefault<string>("Name");

                if (name == null) {
                    UEssentials.Logger.LogError($"Missing required attribute 'Name' in the kit at index {kitObj.Path}.");
                    continue;
                }

                if (loadedKits.ContainsKey(name.ToLowerInvariant())) {
                    UEssentials.Logger.LogWarning($"Duplicated kit ({name})");
                    continue;
                }

                var kit = new Kit(
                    name,
                    GetKitObjValueOrDefault<uint>("Cooldown"),
                    GetKitObjValueOrDefault<decimal>("Cost"),
                    GetKitObjValueOrDefault<bool>("ResetCooldownWhenDie")
                );

                foreach (var itemObj in kitObj.GetValue("Items", StringComparison.InvariantCultureIgnoreCase).Children<JObject>()) {
                    var kitItem = ParseKitItem(kit, itemObj);
                    if (kitItem != null) {
                        kit.Items.Add(kitItem);
                    }
                }
                loadedKits.Add(kit.Name.ToLowerInvariant(), kit);
            }

            return loadedKits;
        }

        private AbstractKitItem ParseKitItem(Kit kit, JObject itemObj) {
            const StringComparison strCmp = StringComparison.InvariantCultureIgnoreCase;

            if (itemObj.TryGetValue("money", strCmp, out var moneyToken)) {
                if (!UEssentials.EconomyProvider.IsPresent) {
                    UEssentials.Logger.LogWarning("Cannot add 'Money' item because there is no active economy system.");
                    return null;
                }
                return new KitItemMoney(moneyToken.Value<decimal>());
            }

            if (itemObj.TryGetValue("xp", strCmp, out var expToken)) {
                return new KitItemExperience(expToken.Value<uint>());
            }

            if (itemObj.TryGetValue("vehicle", strCmp, out var vehicleIdToken)) {
                var vehicleId = vehicleIdToken.Value<ushort>();

                if (Assets.find(EAssetType.VEHICLE, vehicleId) == null) {
                    UEssentials.Logger.LogWarning($"Invalid vehicle id '{vehicleId}' in the item at {itemObj.Path} in the kit '{kit.Name}'");
                    return null;
                }

                return new KitItemVehicle(vehicleId);
            }

            var itemIdToken = itemObj.GetValue("id", strCmp);

            if (itemIdToken == null) {
                UEssentials.Logger.LogWarning($"Missing attribute 'Id' in the item at {itemObj.Path} in the kit '{kit.Name}'");
                return null;
            }

            var itemId = itemIdToken.Value<ushort>();
            var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, itemId);

            var tokKitItemDurability = itemObj.GetValue("Durability", strCmp);
            var tokKitItemAmount = itemObj.GetValue("Amount", strCmp);
            var tokAmmo = itemObj.GetValue("Ammo", strCmp);

            var kitItemAmount = tokKitItemAmount?.Value<byte>() ?? 1;
            var kitItemDurability = tokKitItemDurability?.Value<byte>() ?? 100;

            // Parse weapon specific attributes
            if (itemAsset is ItemGunAsset) {
                var tokFireMode = itemObj.GetValue("FireMode", strCmp);

                EFiremode? fireMode = null;

                if (tokFireMode != null) {
                    try {
                        fireMode = (EFiremode) Enum.Parse(typeof(EFiremode), tokFireMode.Value<string>(), true);
                    } catch (ArgumentException) {
                        UEssentials.Logger.LogWarning($"Invalid firemode '{tokFireMode}' in the item at {itemObj.Path} in the kit '{kit.Name}'. " +
                                                      $"Valid Firemodes: ${string.Join(", ", Enum.GetNames(typeof(EFiremode)))}");
                    }
                }

                var weaponItem = new KitItemWeapon(itemId,
                    kitItemDurability,
                    kitItemAmount,
                    tokAmmo?.Value<byte>() ?? null,
                    fireMode
                )
                {
                    Barrel = itemObj.GetValue("Barrel", strCmp)?.ToObject<Attachment>(),
                    Sight = itemObj.GetValue("Sight", strCmp)?.ToObject<Attachment>(),
                    Tactical = itemObj.GetValue("Tactical", strCmp)?.ToObject<Attachment>(),
                    Grip = itemObj.GetValue("Grip", strCmp)?.ToObject<Attachment>(),
                    Magazine = itemObj.GetValue("Magazine", strCmp)?.ToObject<Attachment>()
                };
                return weaponItem;
            }

            if (itemAsset is ItemMagazineAsset || itemAsset is ItemSupplyAsset) {
                var magazineAmmo = tokAmmo?.Value<byte>() ?? itemAsset.amount;
                return new KitItemMagazine(itemId, kitItemDurability, kitItemAmount, magazineAmmo);
            }

            var kitItem = new KitItem(itemId, kitItemDurability, kitItemAmount);
            if (itemAsset is ItemFuelAsset) {
                var item = kitItem;
                var fuelPercentage = itemObj.GetValue("FuelPercentage", strCmp)?.Value<float>() ?? 100;
                ItemUtil.Refuel(item.Metadata, item.Id, fuelPercentage);
            }
            return kitItem;
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