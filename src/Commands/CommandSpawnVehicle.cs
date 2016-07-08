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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
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
                var target = UPlayer.From(args[1].ToString());
                var vehId = args[0];

                if (target == null) {
                    return CommandResult.Lang(EssLang.PLAYER_NOT_FOUND, args[1]);
                }

                if (!vehId.IsUshort || !IsValidVehicleId(vehId.ToUshort)) {
                    EssLang.INVALID_VEHICLE_ID.SendTo(src, vehId);
                } else {
                    VehicleTool.giveVehicle(target.UnturnedPlayer, vehId.ToUshort);
                    EssLang.SPAWNED_VEHICLE_AT_PLAYER.SendTo(src, args[1]);
                }
            } else if (args.Length == 4) {
                var pos = args.GetVector3(1);
                var vehId = args[0];

                if (pos.HasValue) {
                    var pVal = pos.Value;

                    if (!vehId.IsUshort || !IsValidVehicleId(vehId.ToUshort)) {
                        return CommandResult.Lang(EssLang.INVALID_VEHICLE_ID, vehId);
                    }

                    SpawnVehicle(pVal, vehId.ToUshort);
                    EssLang.SPAWNED_VEHICLE_AT_POSITION.SendTo(src, pVal.x, pVal.y, pVal.z);
                } else {
                    return CommandResult.Lang(EssLang.INVALID_COORDS, args[1], args[2], args[3]);
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

        private static bool IsValidVehicleId(ushort id) {
            return Assets.find(EAssetType.VEHICLE, id) is VehicleAsset;
        }

    }

}