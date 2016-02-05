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
using UnityEngine;
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "jump",
        Description = "Jump to position that you are looking.",
        Usage = "<max_distance>",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandJump : EssCommand
    {
        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            var player = src.ToPlayer();
            var dist = 1000f;

            if ( args.Length == 1 )
            {
                if ( !args[0].IsDouble )
                    goto usage;
                dist = (float) args[0].ToDouble;
            }

            var aim = player.Look.aim;
            var masks = RayMasks.BLOCK_COLLISION & ~(1 << 0x15);
   
            RaycastHit hit;

            Physics.Raycast( aim.position, aim.forward, out hit, dist, masks );
        
            var point = hit.point;

            if ( point == Vector3.zero )
            {
                EssLang.JUMP_NO_POSITION.SendTo( src );
            }
            else
            {
                point.y += 6;
                player.Teleport( point );
                EssLang.JUMPED.SendTo( src, point.x, point.y, point.z );
            }
            return;

            usage:
            ShowUsage( src );
        }
    }
}
