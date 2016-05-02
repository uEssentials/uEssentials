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

using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Components.Player;
using Essentials.Event.Handling;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "freeze",
        Usage = "[player/*]",
        Description = "Freeze a player/everyone"
    )]
    public class CommandFreeze : EssCommand
    {
        public override CommandResult OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.Length == 0 )
            {
                return CommandResult.ShowUsage();
            }
            if ( args[0].Is( "*" ) )
            {
                UServer.Players
                    .Where( player => !player.HasComponent<FrozenPlayer>() )
                    .ForEach( player => {
                        player.AddComponent<FrozenPlayer>();
                        EssLang.FROZEN_PLAYER.SendTo( player, src.DisplayName );    
                    });

                EssLang.FROZEN_ALL.SendTo( src );
            }
            else
            {
                var found = UPlayer.TryGet( args[0], player => {
                    if ( player.HasComponent<FrozenPlayer>() )
                    {
                        EssLang.ALREADY_FROZEN.SendTo( src, player.DisplayName );
                    }
                    else
                    {
                        player.AddComponent<FrozenPlayer>();

                        EssLang.FROZEN_SENDER.SendTo( src, player.DisplayName );
                        EssLang.FROZEN_PLAYER.SendTo( player, src.DisplayName );
                    }
                } );

                if ( !found )
                {
                    return CommandResult.Lang( EssLang.PLAYER_NOT_FOUND, args[0] );
                }
            }

            return CommandResult.Success();
        }

        protected override void OnUnregistered() 
        {
            UEssentials.EventManager.Unregister<EssentialsEventHandler>( "FreezePlayerDisconnect" );
            UEssentials.EventManager.Unregister<EssentialsEventHandler>( "FreezePlayerDeath" );
        }
    }
}
