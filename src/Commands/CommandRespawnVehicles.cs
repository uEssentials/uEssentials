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


using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using SDG.Unturned;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "respawnvehicles",
        Description = "Respawn all vehicles"
    )]
    public class CommandRespawnVehicles : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var respawnedCount = 0;

            UWorld.Vehicles.Where(z => z.isDead).ForEach(vehicle => {
                VehicleManager.sendVehicleRecov(vehicle, vehicle.transform.position, 0);
                respawnedCount++;
            });

            EssLang.Send(src, "RESPAWNED_VEHICLES", respawnedCount);

            return CommandResult.Success();
        }

    }

}