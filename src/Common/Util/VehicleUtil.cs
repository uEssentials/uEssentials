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
    public static class VehicleUtil
    {
        public static Optional<VehicleAsset> GetVehicle( ushort id )
        {
            return Optional<VehicleAsset>.OfNullable( (VehicleAsset) Assets.find( EAssetType.VEHICLE, id ) );
        }

        public static Optional<VehicleAsset> GetVehicle( string name )
        {
            if ( name == null )
            {
                return Optional<VehicleAsset>.Empty();
            }

            ushort id;

            if ( ushort.TryParse( name, out id ) )
            {
                return GetVehicle( id );
            }

            int lastPriority = 0;
            VehicleAsset lastAsset = null;

            Assets.find( EAssetType.VEHICLE )
            .Cast<VehicleAsset>()
            .Where( i => i.Name != null)
            .ForEach( i => {
                var itemPriority = 0;
                var itemName = i.Name;
                
                if ( itemName.EqualsIgnoreCase( name ) )
                {
                    itemPriority = 3;
                }
                else if ( itemName.StartsWith( name ) )
                {
                    itemPriority = 2;
                }
                else if ( (name.Contains( " " ) && 
                          name.Split( ' ' ).All( p => i.Name.ContainsIgnoreCase( p ) ) &&
                          name.Split( ' ' ).Length == itemName.Split( ' ').Length ) ||
                          i.Name.ContainsIgnoreCase( name ) )
                {
                    itemPriority = 1;
                }
                
                if ( itemPriority > lastPriority )
                {
                    lastAsset = i;
                }
            });

            return Optional<VehicleAsset>.OfNullable( lastAsset );
        }
    }
}