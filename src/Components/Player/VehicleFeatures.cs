#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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

        private InteractableVehicle _lastVehicle;
        private int _needRefuelPercentage;
        private int _needRepairPercentage;

        protected override void SafeFixedUpdate() {
            var currentVeh = Player.CurrentVehicle;
            if (currentVeh == null) {
                return;
            }

            if (_lastVehicle != currentVeh) {
                var vehFeatures = UEssentials.Config.VehicleFeatures;
                _lastVehicle = currentVeh;
                _needRefuelPercentage = (_lastVehicle.asset.fuel * vehFeatures.RefuelPercentage) / 100;
                _needRepairPercentage = (_lastVehicle.asset.health * vehFeatures.RepairPercentage) / 100;
            }

            if (AutoRefuel && _lastVehicle.fuel <= _needRefuelPercentage) {
                VehicleManager.sendVehicleFuel(_lastVehicle, _lastVehicle.asset.fuel);
                _lastVehicle.fuel = _lastVehicle.asset.fuel;
            }

            if (AutoRepair && _lastVehicle.health <= _needRepairPercentage) {
                VehicleManager.sendVehicleHealth(_lastVehicle, _lastVehicle.asset.health);
                _lastVehicle.health = _lastVehicle.asset.health;
            }
        }

    }

}