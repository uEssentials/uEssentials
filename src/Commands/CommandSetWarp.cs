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

using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using Essentials.Warps;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "setwarp",
        Description = "Set an warp.",
        Usage = "[warp_name] <x> <y> <z>"
    )]
    public class CommandSetWarp : EssCommand
    {
        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            switch ( parameters.Length )
            {
                case 1:
                    if ( source.IsConsole )
                    {
                        ShowUsage( source );
                        break;
                    }

                    var player = source.ToPlayer();
                    var warp = new Warp( parameters[0].ToString(),  player.Position, player.Rotation );
                    EssProvider.WarpManager.Add( warp );
                    EssLang.WARP_SET.SendTo( source, parameters[0] );
                    break;

                case 4:
                    var pos = parameters.GetVector3( 1 );

                    if ( pos.HasValue )
                    {
                        warp = new Warp( parameters[0].ToString(), pos.Value, 0.0F );

                        EssProvider.WarpManager.Add( warp );

                        EssLang.WARP_SET.SendTo( source, parameters[0] );
                    }
                    else
                    {
                        EssLang.INVALID_COORDS.SendTo( source, parameters[1], parameters[2], parameters[3] );
                    }
                    break;

                default:
                    ShowUsage( source );
                    break;
            }
        }
    }
}
