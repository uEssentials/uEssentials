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

using System.Linq;
using SDG.Unturned;

namespace Essentials.Common.Util
{
    public static class ItemUtil
    {
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

            var asset = (
                from it in Assets.find( EAssetType.ITEM ) 
                let itemAsset = (ItemAsset) it
                where itemAsset.Name != null
                where itemAsset.Name.ToLower().Contains( name.ToLower() )
                select itemAsset
            ).FirstOrDefault();

            return Optional<ItemAsset>.OfNullable( asset );
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
    }
}