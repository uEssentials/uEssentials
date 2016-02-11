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

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "position",
        Aliases = new[] { "pos", "coords" },
        Description = "View your/another player position.",
        Usage = "<player>"
    )]
    public class CommandPosition : EssCommand
    {
        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.Length == 0 )
            {
                if ( source.IsConsole )
                {
                    ShowUsage( source );
                }
                else
                {
                    var player = source.ToPlayer();

                    EssLang.POSITION.SendTo( player, 
                                          player.Position.x,
                                          player.Position.y, 
                                          player.Position.z );
                }
            }
            else
            {
                var found = UPlayer.TryGet( parameters[0], p =>
                {
                    EssLang.POSITION_OTHER.SendTo( source,
                        p.DisplayName, p.Position.x, p.Position.y, p.Position.z );
                } );

                if ( !found )
                {
                    EssLang.PLAYER_NOT_FOUND.SendTo( source, parameters[0] );
                } 
            }
        }
    }
}
