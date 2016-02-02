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

namespace Essentials.Kits
{
    public class JsonKitData : IData<Dictionary<string, Kit>>
    {
        private static string DataFilePath => $"{EssProvider.PluginFolder}kits.json";

        public JsonKitData()
        {
            if ( !File.Exists( DataFilePath ) )
            {
                File.Create( DataFilePath ).Close();
            }
        }

        public void Save( Dictionary<string, Kit> type )
        {
            JsonUtil.Serialize( DataFilePath, type.Values );
        }

        public Dictionary<string, Kit> Load()
        {
            var loadedKits = new Dictionary<string, Kit>();
            var rawJsonText = File.ReadAllText( DataFilePath );

            if ( rawJsonText.Length < 2 )
                return loadedKits;

            var kitArr = JArray.Parse( rawJsonText );
            const StringComparison strCmp = StringComparison.InvariantCultureIgnoreCase;

            foreach ( var kitObj in kitArr.Children<JObject>() )
            {
                var kit = new Kit
                (
                    kitObj.GetValue( "Name", strCmp ).Value<string>(),
                    kitObj.GetValue( "Cooldown", strCmp ).Value<uint>(),
                    kitObj.GetValue( "ResetCooldownWhenDie", strCmp ).Value<bool>()
                );
                
                var itemIndex = 0;

                foreach ( var itemObj in kitObj.GetValue( "items", strCmp ).Children<JObject>() )
                {
                    KitItem kitItem;
                    var tokKitItemId         = itemObj.GetValue( "id",   strCmp );
                    var tokKitItemDurability = itemObj.GetValue( "Durability", strCmp );
                    var tokKitItemAmount     = itemObj.GetValue( "Amount", strCmp );

                    var itemAsset = (ItemAsset) Assets.find( EAssetType.ITEM, 
                        tokKitItemId?.Value<byte>() ?? 0 );

                    if ( tokKitItemId == null || itemAsset == null )
                    {
                        Console.WriteLine( $"Invalid item id. Kit: {kit.Name}" +
                            $"Item Index: {itemIndex++}");
                        continue;
                    }

                    var kitItemId = tokKitItemId.Value<byte>();
                    var kitItemAmount = tokKitItemAmount?.Value<byte>() ?? 1; 
                    var kitItemDurability = tokKitItemDurability?.Value<byte>() ?? 100;

                    if ( itemAsset.UseableType == EUseableType.GUN )
                        goto weaponItem;

                    kitItem = new KitItem( kitItemId, kitItemDurability, kitItemAmount )
                    {
                        Type = itemAsset.UseableType == EUseableType.CLOTHING
                               ? KitItemType.CLOTHING
                               : KitItemType.NORMAL
                    };
                    goto add;     

                    weaponItem:
                    var tokFireMode    = itemObj.GetValue( "FireMode", strCmp );
                    var tokBarrel      = itemObj.GetValue( "Barrel", strCmp );
                    var tokSight       = itemObj.GetValue( "Sight", strCmp );
                    var tokGrip        = itemObj.GetValue( "Grip", strCmp );
                    var tokMagazine    = itemObj.GetValue( "Magazine", strCmp );
                    var tokTatical     = itemObj.GetValue( "Tatical", strCmp );
                    var tokAmmo        = itemObj.GetValue( "Ammo", strCmp );

                    var fireMode = EFiremode.AUTO;
                    var ammo     = tokAmmo?.Value<byte>() ?? 0;

                    try
                    {
                        fireMode = (EFiremode) Enum.Parse( typeof (EFiremode),
                            tokFireMode.Value<string>(), true );
                    }
                    catch ( ArgumentException )
                    {
                        EssProvider.Logger.LogWarning( $"Invalid firemode '{tokFireMode.Value<string>()}' " +
                                                      $"in kit '{kit.Name}', item '{itemIndex + 1}'!" );
                    }

                    kitItem = new KitItemWeapon( kitItemId,
                                                 kitItemDurability,
                                                 kitItemAmount,
                                                 ammo,
                                                 fireMode );

                    var weaponItem = (KitItemWeapon) kitItem;

                    Func<JToken, Attachment> deserializeAttach = json => json != null 
                        ? JsonConvert.DeserializeObject<Attachment>( json.ToString() )
                        : null;

                    weaponItem.Barrel    = deserializeAttach( tokBarrel );
                    weaponItem.Sight     = deserializeAttach( tokSight );
                    weaponItem.Tatical   = deserializeAttach( tokTatical );
                    weaponItem.Grip      = deserializeAttach( tokGrip );
                    weaponItem.Magazine  = deserializeAttach( tokMagazine );

                    add:
                    kit.Items.Add( kitItem );
                }
                loadedKits.Add( kit.Name, kit );
            }

            return loadedKits;
        }
    }
}
