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

namespace Essentials.Components.Player
{
    public class PlayerVehicleFeatures : PlayerComponent
    {
        public bool AutoRefuel { get; set; }
        public bool AutoRepair { get; set; }

        private void FixedUpdate()
        {
            if ( !Player.IsInVehicle )
            {
                return;
            }

            var veh = Player.CurrentVehicle;

            if ( veh.fuel < 100 && AutoRefuel )
            {
                VehicleManager.sendVehicleFuel( veh, veh.asset.fuel );
                veh.fuel = veh.asset.fuel;
            }

            if ( !AutoRepair )
            {
                return;
            }

            VehicleManager.sendVehicleHealth( veh, veh.asset.health );
            veh.health = veh.asset.health;
        }
    }
}
