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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "spawnvehicle",
        Aliases = new[] { "spawnveh" },
        Usage = "[id] [player] or [x] [y] [z]",
        Description = "Spawn a vehicle on player's/given position"
    )]
    public class CommandSpawnVehicle : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.Length == 2) {
                var vehId = args[0];

                if (!args[1].IsValidPlayerIdentifier) {
                    return CommandResult.LangError("PLAYER_NOT_FOUND", args[1]);
                }

                var target = args[1].ToPlayer;

                if (!vehId.IsUShort || !IsValidVehicleId(vehId.ToUShort)) {
                    EssLang.Send(src, "INVALID_VEHICLE_ID", vehId);
                } else {
                    VehicleTool.giveVehicle(target.UnturnedPlayer, vehId.ToUShort);
                    EssLang.Send(src, "SPAWNED_VEHICLE_AT_PLAYER", args[1]);
                }
            } else if (args.Length == 4) {
                var pos = args.GetVector3(1);
                var vehId = args[0];

                if (pos.HasValue) {
                    var pVal = pos.Value;

                    if (!vehId.IsUShort || !IsValidVehicleId(vehId.ToUShort)) {
                        return CommandResult.LangError("INVALID_VEHICLE_ID", vehId);
                    }

                    SpawnVehicle(pVal, vehId.ToUShort);
                    EssLang.Send(src, "SPAWNED_VEHICLE_AT_POSITION", pVal.x, pVal.y, pVal.z);
                } else {
                    return CommandResult.LangError("INVALID_COORDS", args[1], args[2], args[3]);
                }
            } else {
                return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

        private static void SpawnVehicle(Vector3 pos, ushort id) {
            RaycastHit raycastHit;
            Physics.Raycast(pos + Vector3.up*16f, Vector3.down, out raycastHit, 32f, RayMasks.BLOCK_VEHICLE);

            if (raycastHit.collider != null) {
                pos.y = raycastHit.point.y + 16f;
            }

            VehicleManager.spawnVehicle(id, pos, Quaternion.identity);
        }

        private static bool IsValidVehicleId(ushort id) => Assets.find(EAssetType.VEHICLE, id) is VehicleAsset;

    }

}