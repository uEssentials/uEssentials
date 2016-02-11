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

using SDG.Unturned;

// ReSharper disable RedundantCheckBeforeAssignment

namespace Essentials.Core.Components.Player
{
    public class PlayerVehicleFeatures : PlayerComponent
    {
        private InteractableVehicle cachedVeh;

        public bool AutoRefuel { get; set; }
        public bool AutoRepair { get; set; }

        public PlayerVehicleFeatures()
        {
            cachedVeh = Player.CurrentVehicle;
        }
        
        private void FixedUpdate()
        {
            if ( !Player.IsInVehicle )
            {
                return;
            }

            if ( cachedVeh != Player.CurrentVehicle )
            {
                cachedVeh = Player.CurrentVehicle;
            }

            if ( cachedVeh.fuel < 100 && AutoRefuel )
            {
                VehicleManager.sendVehicleFuel( cachedVeh, cachedVeh.asset.fuel );
                cachedVeh.fuel = cachedVeh.asset.fuel;
            }

            if ( !AutoRepair )
            {
                return;
            }

            VehicleManager.sendVehicleHealth( cachedVeh, cachedVeh.asset.health );
            cachedVeh.health = cachedVeh.asset.health;
        }
    }
}