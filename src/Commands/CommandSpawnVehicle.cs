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

using System;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "spawnvehicle",
        Aliases = new [] {"spawnveh"},
        Usage = "[id] [player] or [x] [y] [z]",
        Description = "Spawn an vehicle at given player/position"
    )]
    public class CommandSpawnVehicle : EssCommand
    {
        private static void SpawnVehicle( Vector3 pos, ushort id )
        {
            RaycastHit raycastHit;
            Physics.Raycast( pos + Vector3.up*16f, Vector3.down, out raycastHit, 32f, RayMasks.BLOCK_VEHICLE );

            if ( raycastHit.collider != null )
            {
                pos.y = raycastHit.point.y + 16f;
            }

            VehicleManager.spawnVehicle( id, pos, Quaternion.identity );
        }

        private static bool IsValidVehicleId( ushort id )
        {
            return Assets.find( EAssetType.VEHICLE, id ) is VehicleAsset;
        }


        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            switch ( args.Length )
            {
                case 2:
                    var found = UPlayer.TryGet( args[1], player =>
                    {
                        var vehId = args[0];

                        if ( !vehId.IsUshort || !IsValidVehicleId( vehId.ToUshort ) )
                        {
                            EssLang.INVALID_VEHICLE_ID.SendTo( src, vehId );
                        }
                        else
                        {
                            VehicleTool.giveVehicle( player.UnturnedPlayer, vehId.ToUshort );
                            EssLang.SPAWNED_VEHICLE_AT_PLAYER.SendTo( src, args[1] );
                        }
                    });

                    if (!found)
                        EssLang.PLAYER_NOT_FOUND.SendTo( src, args[1] );
                    break;

                case 4:
                    try
                    {
                        var x = (float) args[1].ToDouble;
                        var y = (float) args[2].ToDouble;
                        var z = (float) args[3].ToDouble;

                        var pos = new Vector3( x, y, z );
                        var vehId = args[0];

                        if ( !vehId.IsUshort || !IsValidVehicleId( vehId.ToUshort ) )
                        {
                            EssLang.INVALID_VEHICLE_ID.SendTo( src, vehId );
                        }
                        else
                        {
                            SpawnVehicle( pos, vehId.ToUshort );
                            EssLang.SPAWNED_VEHICLE_AT_POSITION.SendTo( src, x, y, z );
                        }
                    }
                    catch ( FormatException )
                    {
                        EssLang.INVALID_COORDS.SendTo( src, args[1], args[2], args[3] );
                    }
                    break;

                default:
                    ShowUsage( src );
                    break;
            }
        }
    }
}