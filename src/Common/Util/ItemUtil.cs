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
using System.Globalization;
using System.Linq;
using Rocket.Unturned.Items;
using SDG.Unturned;

namespace Essentials.Common.Util
{
    public static class ItemUtil
    {
        private static IOrderedEnumerable<ItemAsset> _cachedAssets;
        
        public static Optional<ItemAsset> GetItem( ushort id )
        {
            return Optional<ItemAsset>.OfNullable( (ItemAsset) Assets.find( EAssetType.ITEM, id ) );
        }

        public static Optional<ItemAsset> GetItem( string name )
        {
            if ( name == null )
            {
                return Optional<ItemAsset>.Empty();
            }

            ushort id;

            if ( ushort.TryParse( name, out id ) )
            {
                return GetItem( id );
            }

            if ( _cachedAssets == null )
            {
                _cachedAssets = Assets.find( EAssetType.ITEM )
                                  .Cast<ItemAsset>()
                                  .Where( i => i.Name != null )
                                  .OrderBy( i => i.Id );
            }

            var lastAsset = null as ItemAsset;
            int lastPriority = 0;

            foreach ( var asset in _cachedAssets )
            {
                var itemPriority = 0;
                var itemName = asset.Name;
                
                if ( itemName.EqualsIgnoreCase( name ) )
                {
                    lastAsset = asset;
                    break;
                }

                if ( itemName.StartsWith( name, true, CultureInfo.InvariantCulture ) )
                {
                    itemPriority = 3;
                }
                else if ( itemName.ContainsIgnoreCase( name ) )
                {
                    itemPriority = 2;
                }
                else if ( name.Contains( " " ) && 
                          name.Split( ' ' ).All( p => itemName.ContainsIgnoreCase( p ) ) )
                {
                    itemPriority = 1;
                }

                if ( itemPriority > lastPriority )
                {
                    lastAsset = asset;
                    lastPriority = itemPriority;
                }
            }
            
            return Optional<ItemAsset>.OfNullable( lastAsset );
        }

        public static Optional<T> GetItemAs<T>( string name ) where T : ItemAsset
        {
            var optItem = GetItem( name );

            return optItem.IsPresent ? Optional<T>.Of( optItem.Value as T ) : Optional<T>.Empty();
        }

        public static Optional<T> GetItemAs<T>( ushort id ) where T : ItemAsset
        {
            var optItem = GetItem( id );

            return optItem.IsPresent ? Optional<T>.Of( optItem.Value as T ) : Optional<T>.Empty();
        }


        public static Optional<Attachment> GetWeaponAttachment( Item weaponItem, AttachmentType type )
        {
            return GetWeaponAttachment( weaponItem.Metadata, type );
        }

        public static Optional<Attachment> GetWeaponAttachment( byte[] metadata, AttachmentType type )
        {
            if ( metadata.Length < 18 )
            {
                return Optional<Attachment>.Empty();
            }

            int[] indexes;

            switch (type)
            {
                case AttachmentType.SIGHT:
                    indexes = new[] { 0x0, 0x1, 0xD };
                    break;

                case AttachmentType.TACTICAL:
                    indexes = new[] { 0x2, 0x3, 0xE };
                    break;

                case AttachmentType.GRIP:
                    indexes = new[] { 0x4, 0x5, 0xF };
                    break;

                case AttachmentType.BARREL:
                    indexes = new[] { 0x6, 0x7, 0x10 };
                    break;

                case AttachmentType.MAGAZINE:
                    indexes = new[] { 0x8, 0x9, 0x11 };
                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof( type ), type, null );
            }

            var attachDurability = metadata[indexes[2]];
            var attachId = BitConverter.ToUInt16( new [] {
                metadata[indexes[0]],
                metadata[indexes[1]]
            }, 0 );

            return Optional<Attachment>.Of( new Attachment( attachId, attachDurability ) );
        }

        public static Optional<EFiremode> GetWeaponFiremode( Item weaponItem )
        {
            return GetWeaponFiremode( weaponItem.Metadata );

        }
        public static Optional<EFiremode> GetWeaponFiremode( byte[] metadata )
        {
            if ( metadata.Length < 18 )
            {
                return Optional<EFiremode>.Empty();
            }

            return Optional<EFiremode>.OfNullable( (EFiremode) metadata[0xB] );
        }

        public static Optional<byte> GetWeaponAmmo(Item weaponItem)
        {
            return GetWeaponAmmo( weaponItem.Metadata );
        }

        public static Optional<byte> GetWeaponAmmo( byte[] metadata )
        {
            if ( metadata.Length < 18 )
            {
                return Optional<byte>.Empty();
            }

            return Optional<byte>.Of( metadata[0xA] );
        }


        public static void SetWeaponAttachment( Item weaponItem, AttachmentType type, Attachment attach )
        {
            if ( weaponItem.Metadata.Length < 18 )
            {
                return;
            }

            switch (type)
            {
                case AttachmentType.SIGHT:
                    AssembleAttach( weaponItem, new[] { 0x0, 0x1, 0xD }, attach );
                    break;

                case AttachmentType.TACTICAL:
                    AssembleAttach( weaponItem, new[] { 0x2, 0x3, 0xE }, attach );
                    break;

                case AttachmentType.GRIP:
                    AssembleAttach( weaponItem, new[] { 0x4, 0x5, 0xF }, attach );
                    break;

                case AttachmentType.BARREL:
                    AssembleAttach( weaponItem, new[] { 0x6, 0x7, 0x10 }, attach );
                    break;

                case AttachmentType.MAGAZINE:
                    AssembleAttach( weaponItem, new[] { 0x8, 0x9, 0x11 }, attach );
                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof( type ), type, null );
            }
        }

        public static void SetWeaponFiremode( Item weaponItem, EFiremode firemode )
        {
            if ( weaponItem.Metadata.Length < 18 )
            {
                return;
            }

            weaponItem.Metadata[0xB] = (byte) firemode;
        }

        public static void SetWeaponAmmo( Item weaponItem, byte ammo )
        {
            if ( weaponItem.Metadata.Length < 18 )
            {
                return;
            }

            weaponItem.Metadata[0xA] = ammo;   
        }

        private static void AssembleAttach( Item item, IList<int> indexes, Attachment attach )
        {
            if ( attach == null || attach.AttachmentId == 0 ) return;

            var attachIdBytes = BitConverter.GetBytes( attach.AttachmentId );

            item.Metadata[indexes[0]] = attachIdBytes[0];
            item.Metadata[indexes[1]] = attachIdBytes[1];
            item.Metadata[indexes[2]] = attach.Durability;
        }

        public enum AttachmentType
        {
            SIGHT,
            BARREL,
            GRIP,
            TACTICAL,
            MAGAZINE
        }
    }
}