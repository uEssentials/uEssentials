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
using Essentials.I18n;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{
    public class SmallCommands
    {
        [CommandInfo(
            Name = "ascend",
            Aliases = new []{"asc"},
            Usage = "[amount]",
            Description = "Ascend X \"meters\".",
            AllowedSource = AllowedSource.PLAYER
        )]
        public void AscendCommand( ICommandSource src, ICommandArgs args, ICommand cmd )
        {
            if ( args.IsEmpty )
            {
                src.SendMessage( $"Use /{cmd.Name} {cmd.Usage}" );
            }
            else if ( !args[0].IsFloat )
            {
                EssLang.INVALID_NUMBER.SendTo( src, args[0] );
            }
            else if ( args[0].ToFloat <= 0 )
            {
                EssLang.MUST_POSITIVE.SendTo( src, args[0] );
            }
            else
            {
                var player = src.ToPlayer();
                var pos = new Vector3(player.Position.x, player.Position.y, player.Position.z);
                var num = args[0].ToFloat;

                pos.y += num;

                player.Teleport( pos );
                player.SendMessage( $"You ascended {num} \"meters\"" );
            }
        }

        [CommandInfo(
            Name = "descend",
            Aliases = new[] {"desc"},
            Usage = "[amount]",
            Description = "Descend X \"meters\".",
            AllowedSource = AllowedSource.PLAYER
        )]
        public void DescendCommand( ICommandSource src, ICommandArgs args, ICommand cmd )
        {
            if ( args.IsEmpty )
            {
                src.SendMessage( $"Use /{cmd.Name} {cmd.Usage}" );
            }
            else if ( !args[0].IsFloat )
            {
                EssLang.INVALID_NUMBER.SendTo( src, args[0] );
            }
            else if ( args[0].ToFloat <= 0 )
            {
                EssLang.MUST_POSITIVE.SendTo( src );
            }
            else
            {
                var player = src.ToPlayer();
                var pos = new Vector3( player.Position.x, player.Position.y, player.Position.z );
                var num = args[0].ToFloat;

                pos.y -= num;

                player.Teleport( pos );
                player.SendMessage( $"You descended {num} \"meters\"" );
            }
        }

        [CommandInfo(
            Name = "clear",
            Description = "Clear things",
            Usage = "-i = items, -v = vehicles"
        )]
        public void ClearCommand( ICommandSource src, ICommandArgs args, ICommand cmd )
        {
            if ( args.IsEmpty )
            {
                src.SendMessage( $"Use /{cmd.Name} {cmd.Usage}" );
                return;
            }

            var joinedArgs = args.Join( 0 );

            Func<string, bool> hasArg = arg =>
            {
                return joinedArgs.IndexOf( $"-{arg}", 0, StringComparison.InvariantCultureIgnoreCase ) != -1 ||
                        (joinedArgs.Contains( "-a" ) || joinedArgs.Contains( "-A" ));
            };

            /*
                TODO: Options
                    -i = items
                    -v = vehicles
                    -z = zombies
                    -b = barricades
                    -s = structures
                    -a = ALL
                
                /clear -i -z -v = items, zombies, vehicles
            */

            if ( hasArg( "i" ) )
            {
                ItemManager.askClearAllItems();
                EssLang.CLEAR_ITEMS.SendTo( src );
            }

            if ( hasArg( "v" ) )
            {
                VehicleManager.askVehicleDestroyAll();
                EssLang.CLEAR_VEHICLES.SendTo( src );
            }
        }
    }
}
