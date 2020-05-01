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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "unlock",
        Description = "Unlock any vehicle on sight.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandUnlock : EssCommand
    {
        public RaycastInfo TraceRay(UPlayer player, float distance, int masks)
        {
            return DamageTool.raycast(new Ray(player.Look.aim.position, player.Look.aim.forward), distance, masks);
        }
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();

            float dist = 2048f;
            RaycastInfo veh = TraceRay(player, dist, RayMasks.VEHICLE);

            if (veh.vehicle != null)
            {
                if (veh.vehicle.isLocked) 
                { 
                    VehicleManager.unlockVehicle(veh.vehicle, player.UnturnedPlayer);
                    EssLang.Send(src, "VEHICLE_UNLOCK");
                    return CommandResult.Success();
                }
                else
                {
                    return CommandResult.LangError("VEHICLE_NOT_LOCKED");
                }
            }
            else
            {
                return CommandResult.LangError("NO_OBJECT");
            }
        }
    }

}