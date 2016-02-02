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
using System.Collections.Generic;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using UnityEngine;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "tpall",
        Description = "Teleport all players to an player/position",
        Usage = "[player/position]"
    )]
    public class CommandTpAll : EssCommand
    {
        private static readonly Action<Vector3, List<UPlayer>> TeleportAll = ( pos, players ) =>
        {
            players.ForEach( 
                player => player.UnturnedPlayer.sendTeleport( pos, 0 ) 
            );
        };

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            var players = UServer.Players.ToList();

            if ( players.Count == ( source.IsConsole ? 0 : 1) )
            {
                EssLang.NO_PLAYERS_FOR_TELEPORT.SendTo( source );
            }
            else
            {
                switch ( parameters.Length )
                {
                    case 0:
                        if ( source.IsConsole )
                        {
                            source.SendMessage( "Use /tpall [player/position[x, y, z]]" );
                        }
                        else
                        {
                            TeleportAll( source.ToPlayer().RocketPlayer.Position, players );
                            EssLang.TELEPORTED_ALL_YOU.SendTo( source );
                        }
                        break;
                    case 1:
                        var found = UPlayer.TryGet( parameters[0], player => {
                            TeleportAll( player.Position, players );
                            EssLang.TELEPORTED_ALL_PLAYER.SendTo( source, player.DisplayName );
                        } );

                        if ( !found )
                        {
                            EssLang.PLAYER_NOT_FOUND.SendTo( source, parameters[0].ToString() );
                        }
                        break;
                    case 3:
                        try
                        {
                            var x = (float) parameters[0].ToDouble;
                            var y = (float) parameters[1].ToDouble;
                            var z = (float) parameters[2].ToDouble;

                            var pos = new Vector3( x, y, z );

                            TeleportAll( pos, players );
                            EssLang.TELEPORTED_ALL_COORDS.SendTo( source, x, y, z );
                        }
                        catch ( FormatException )
                        {
                            EssLang.INVALID_COORDS.SendTo( 
                                source, 
                                parameters[0], 
                                parameters[1],
                                parameters[2]
                            );
                        }
                        break;

                    default:
                        source.SendMessage( "Use /tpall [player/position[x, y, z]]" );
                        break;
                }
            }
        }
    }
}
