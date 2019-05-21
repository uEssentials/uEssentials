﻿#region License
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
using SDG.Unturned;

namespace Essentials.Components.Player {

    public class VehicleFeatures : PlayerComponent {

        public bool AutoRefuel { get; set; }
        public bool AutoRepair { get; set; }

        protected override void SafeFixedUpdate() {
            var currentVeh = Player.CurrentVehicle;

            if (currentVeh == null) {
                return;
            }

            var needRepairPercentage = (currentVeh.asset.health * UEssentials.Config.VehicleFeatures.RepairPercentage) / 100;
            var needRefuelPercentage = (currentVeh.asset.fuel * UEssentials.Config.VehicleFeatures.RefuelPercentage) / 100;

            if (AutoRefuel && currentVeh.fuel <= needRefuelPercentage) {
                VehicleManager.sendVehicleFuel(currentVeh, currentVeh.asset.fuel);
                currentVeh.fuel = currentVeh.asset.fuel;
            }

            if (AutoRepair && currentVeh.health <= needRepairPercentage) {
                VehicleManager.sendVehicleHealth(currentVeh, currentVeh.asset.health);
                currentVeh.health = currentVeh.asset.health;
            }
        }

    }

}