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
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "refuelvehicle",
        Aliases = new[] { "refuel" },
        Description = "Refuel current/all vehicles",
        Usage = "<all>",
        MaxArgs = 1
    )]
    public class CommandRefuelVehicle : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty) {
                if (src.IsConsole) {
                    return CommandResult.ShowUsage();
                }

                var currentVeh = src.ToPlayer().CurrentVehicle;

                if (currentVeh != null) {
                    RefuelVehicle(currentVeh);
                    EssLang.Send(src, "VEHICLE_REFUELED");
                } else {
                    return CommandResult.LangError("NOT_IN_VEHICLE");
                }
            } else if (args[0].Equals("all")) {
                if (!src.HasPermission($"{Permission}.all")) {
                    return CommandResult.NoPermission($"{Permission}.all");
                }

                lock (UWorld.Vehicles) {
                    UWorld.Vehicles
                        .Where(veh => !veh.isExploded && !veh.isUnderwater)
                        .ForEach(RefuelVehicle);

                    EssLang.Send(src, "VEHICLE_REFUELED_ALL");
                }
            }

            return CommandResult.Success();
        }

        private void RefuelVehicle(InteractableVehicle veh) {
            VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL,
                ESteamPacket.UPDATE_UNRELIABLE_BUFFER, veh.instanceID, veh.asset.fuel);
        }

    }

}