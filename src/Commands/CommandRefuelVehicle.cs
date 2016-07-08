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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;
using SDG.Unturned;

// ReSharper disable InconsistentNaming

namespace Essentials.Commands {

    [CommandInfo(
        Name = "refuelvehicle",
        Aliases = new[] { "refuel" },
        Description = "Refuel current/all vehicles",
        Usage = "<all>"
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
                    EssLang.VEHICLE_REFUELED.SendTo(src);
                } else {
                    return CommandResult.Lang(EssLang.NOT_IN_VEHICLE);
                }
            } else if (args[0].Is("all")) {
                if (!src.HasPermission(Permission + ".all")) {
                    return CommandResult.Lang(EssLang.COMMAND_NO_PERMISSION);
                }

                lock (UWorld.Vehicles) {
                    UWorld.Vehicles
                        .Where(veh => !veh.isExploded && !veh.isUnderwater)
                        .ToList()
                        .ForEach(RefuelVehicle);

                    EssLang.VEHICLE_REFUELED_ALL.SendTo(src);
                }
            }

            return CommandResult.Success();
        }

        private void RefuelVehicle(InteractableVehicle veh) {
            VehicleManager.Instance.channel.send("tellVehicleFuel", ESteamCall.ALL,
                ESteamPacket.UPDATE_UNRELIABLE_BUFFER, veh.instanceID, veh.asset.fuel);
        }

    }

}