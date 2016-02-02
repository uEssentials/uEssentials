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
using UnityEngine;
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "boom",
        Aliases = new[] { "explode" },
        Description = "Create an explosion an given position/player",
        Usage = "[player|x, y, z]"
    )]
    public class CommandBoom : EssCommand
    {
        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            switch ( args.Length )
            {
                case 1:
                    var found = UPlayer.TryGet( args[0], player => 
                        Explode( player.Position ) 
                    );

                    if ( !found )
                    {
                        EssLang.PLAYER_NOT_FOUND.SendTo( src, args[0] );
                    }
                    break;

                case 3:
                    var rawX = args[0];
                    var rawY = args[1];
                    var rawZ = args[2];

                    if ( !rawX.IsInt || !rawY.IsInt || !rawZ.IsInt )
                    {
                        EssLang.INVALID_COORDS.SendTo( src, rawX, rawY, rawZ );
                    }
                    else
                    {
                        Explode( new Vector3( rawX.ToInt, rawY.ToInt, rawZ.ToInt ) );
                    }
                    break;

                default:
                    ShowUsage( src );
                    return;
            }
        }

        private static void Explode( Vector3 pos )
        {
            EffectManager.sendEffect( 20, EffectManager.INSANE, pos );
            DamageTool.explode( pos, 10f, EDeathCause.GRENADE, 200, 200, 200, 200, 200, 200, 200);
        }
    }
}
