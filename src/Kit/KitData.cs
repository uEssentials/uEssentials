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
using Essentials.Kit.Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Items;
using SDG.Unturned;

namespace Essentials.Kit
{
    public class KitData : IData<Dictionary<string, Kit>>
    {
        protected static string DataFilePath => $"{EssProvider.PluginFolder}kits.json";

        public virtual void Save( Dictionary<string, Kit> type )
        {
            JsonUtil.Serialize( DataFilePath, type.Values );
        }

        public virtual Dictionary<string, Kit> Load()
        {
            var loadedKits = new Dictionary<string, Kit>();

            if ( !File.Exists( DataFilePath ) )
            {
                File.Create( DataFilePath ).Close();
                LoadDefault();
            }
            
            JArray kitArr;

            try
            {
                kitArr = JArray.Parse( File.ReadAllText( DataFilePath ) );
            }
            catch (JsonReaderException ex)
            {
                EssProvider.Logger.LogError( $"Invalid kit configuration ({DataFilePath})" );
                EssProvider.Logger.LogError( ex.Message );
                kitArr = JArray.Parse( "[]" );
            }

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
                    AbstractKitItem kitItem;

                    if ( itemObj.GetValue( "xp", strCmp ) != null )
                    {
                        kitItem = new KitItemExperience( itemObj.GetValue( "xp", strCmp ).Value<uint>() );
                        goto add;
                    }

                    if ( itemObj.GetValue( "vehicle", strCmp ) != null)
                    {
                        var vehicleId = itemObj.GetValue( "vehicle", strCmp ).Value<ushort>();

                        if ( Assets.find( EAssetType.VEHICLE, vehicleId ) == null )
                        {
                            EssProvider.Logger.LogWarning( $"Invalid vehicle id. Kit: {kit.Name} item Index: {itemIndex++}");
                            continue;
                        }

                        kitItem = new KitItemVehicle( vehicleId );
                        goto add;
                    }

                    var tokKitItemId         = itemObj.GetValue( "id",   strCmp );
                    var tokKitItemDurability = itemObj.GetValue( "Durability", strCmp );
                    var tokKitItemAmount     = itemObj.GetValue( "Amount", strCmp );
                    var tokAmmo              = itemObj.GetValue( "Ammo", strCmp );

                    var itemAsset = (ItemAsset) Assets.find( EAssetType.ITEM, 
                        tokKitItemId?.Value<ushort>() ?? 0 );

                    if ( tokKitItemId == null || itemAsset == null )
                    {
                        EssProvider.Logger.LogWarning( $"Invalid item id. Kit: {kit.Name} item Index: {itemIndex++}");
                        continue;
                    }

                    var kitItemId = tokKitItemId.Value<ushort>();
                    var kitItemAmount = tokKitItemAmount?.Value<byte>() ?? 1; 
                    var kitItemDurability = tokKitItemDurability?.Value<byte>() ?? 100;

                    if ( itemAsset.UseableType == EUseableType.GUN )
                        goto weaponItem;

                    if ( itemAsset is ItemMagazineAsset || itemAsset is ItemSupplyAsset  )
                    {
                        kitItem = new KitItemMagazine( kitItemId, kitItemDurability, kitItemAmount, tokAmmo?.Value<byte>() ?? 1 );
                    }
                    else
                    {
                        kitItem = new KitItem( kitItemId, kitItemDurability, kitItemAmount );
                        
                        if ( itemAsset is ItemFuelAsset )
                        {
                            ((KitItem) kitItem).Metadata[0] = 1;
                        }
                    }
                    goto add;     
                    
                    weaponItem:
                    var tokFireMode    = itemObj.GetValue( "FireMode", strCmp );
                    var tokBarrel      = itemObj.GetValue( "Barrel", strCmp );
                    var tokSight       = itemObj.GetValue( "Sight", strCmp );
                    var tokGrip        = itemObj.GetValue( "Grip", strCmp );
                    var tokMagazine    = itemObj.GetValue( "Magazine", strCmp );
                    var tokTactical    = itemObj.GetValue( "Tatical", strCmp ) ?? itemObj.GetValue( "Tactical", strCmp );

                    EFiremode? fireMode = null;
                    var ammo            = tokAmmo?.Value<byte>() ?? null;

                    if ( tokFireMode != null )
                    {
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
                    }

                    kitItem = new KitItemWeapon( kitItemId,
                                                    kitItemDurability,
                                                    kitItemAmount,
                                                    ammo,
                                                    fireMode );

                    var weaponItem = (KitItemWeapon) kitItem;

                    Func<JToken, Attachment> deserializeAttach = json =>
                    {
                        return json == null ? null : JsonConvert.DeserializeObject<Attachment>( json.ToString() );
                    };
           
                    weaponItem.Barrel    = deserializeAttach( tokBarrel );
                    weaponItem.Sight     = deserializeAttach( tokSight );
                    weaponItem.Tactical  = deserializeAttach( tokTactical );
                    weaponItem.Grip      = deserializeAttach( tokGrip );
                    weaponItem.Magazine  = deserializeAttach( tokMagazine );

                    add:
                    kit.Items.Add( kitItem );
                }
                loadedKits.Add( kit.Name.ToLowerInvariant(), kit );
            }

            return loadedKits;
        }

        private void LoadDefault()
        {
            var defaultKits = new Dictionary<string, Kit>();

            var defaultKit = new Kit( "default", 120, true );
            var weaponKit = new Kit( "default2", 1200, false );
            var planeKit = new Kit( "plane", 9000, false );
            var xpKit = new Kit( "xp", 1200, false );

            defaultKit.Items.Add( new KitItem( 16, 100, 1 ) );
            defaultKit.Items.Add( new KitItem( 13, 100, 2 ) );
            defaultKit.Items.Add( new KitItem( 14, 100, 1 ) );

            weaponKit.Items.Add( new KitItemWeapon( 4, 100, 1, 30, EFiremode.BURST ) {
                Barrel = new Attachment( 7, 100 ),
                Grip = new Attachment( 8, 100 ),
                Sight = new Attachment( 146, 100 ),
                Magazine = new Attachment( 17, 100 ),
                Tactical = new Attachment( 151, 100 )
            } );

            planeKit.Items.Add( new KitItemVehicle( 92 ) );
            xpKit.Items.Add( new KitItemExperience( 100 ) );

            defaultKits.Add( "default", defaultKit );
            defaultKits.Add( "weapon", weaponKit );
            defaultKits.Add( "plane", planeKit );
            defaultKits.Add( "xp", xpKit );

            Save( defaultKits );
        }
    }
}
