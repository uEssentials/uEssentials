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
using Essentials.I18n;

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

            var eyePos = player.GetEyePosition( dist );

            if ( !eyePos.HasValue )
            {
                EssLang.JUMP_NO_POSITION.SendTo( src );
            }
            else
            {
                var point = eyePos.Value;
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
