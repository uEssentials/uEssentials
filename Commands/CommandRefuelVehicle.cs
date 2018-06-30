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

using System;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Common;
using Rocket.API.Commands;
using Rocket.API.Permissions;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Migration.LegacyPermissions;
using Rocket.Core.Permissions;
using Rocket.Core.User;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Object = UnityEngine.Object;

namespace Essentials.Commands
{
    [CommandInfo(
        "refuelvehicle",
        "Refuel current/all vehicles",
        Aliases = new[] {"refuel"},
        Syntax= "<all>"
    )]
    public class CommandRefuelVehicle : EssCommand
    {
        public CommandRefuelVehicle(IPlugin plugin) : base(plugin)
        {
        }
        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
            {
                if (!(context.User is UnturnedUser))
                {
                    throw new CommandWrongUsageException();
                }

                var currentVeh = ((UnturnedUser)context.User).Player.CurrentVehicle;

                if (currentVeh != null)
                {
                    RefuelVehicle(currentVeh);
                    context.User.SendLocalizedMessage(Translations, "VEHICLE_REFUELED");
                }
                else
                {
                    throw new CommandWrongUsageException(Translations.Get("NOT_IN_VEHICLE"));
                }

                return;
            }

            if (context.Parameters[0].Equals("all"))
            {
                if (context.User.CheckPermission($"RefuelVehicle.all") != PermissionResult.Grant)
                {
                    throw new NotEnoughPermissionsException(context.User, "RefuelVehicle.all");
                }

                var vehicles = Object.FindObjectsOfType<InteractableVehicle>();
                lock (vehicles)
                {
                    vehicles
                        .Where(veh => !veh.isExploded && !veh.isUnderwater)
                        .ForEach(RefuelVehicle);

                    context.User.SendLocalizedMessage(Translations, "VEHICLE_REFUELED_ALL");
                }
            }
        }

        private void RefuelVehicle(InteractableVehicle veh)
        {
            VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL,
                ESteamPacket.UPDATE_UNRELIABLE_BUFFER, veh.instanceID, veh.asset.fuel);
        }
    }
}