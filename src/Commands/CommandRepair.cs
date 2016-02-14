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
using Essentials.Common.Reflect;
using SDG.Unturned;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "repair",
        Aliases = new[] { "fix" },
        Description = "Repair all/in hand items",
        AllowedSource = AllowedSource.PLAYER,
        Usage = "[all/hand]"
    )]
    public class CommandRepair : EssCommand
    {
        private static readonly Action<UPlayer, Items> Repair = ( player, item ) =>
        {
            var field = AccessorFactory.AccessField<List<ItemJar>>( item, "items" );
            var items = field.Value;
            byte index = 0;

            items.ForEach( itemJar =>
            {
                item.updateQuality( index, 100 );

                player.UnturnedPlayer.inventory.channel.send("tellUpdateQuality", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				{
    				item.page,
    			 	player.UnturnedPlayer.inventory.getIndex(item.page, itemJar.PositionX, itemJar.PositionY),
    				100
				});

                index++;
            });
        };

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            var player = source.ToPlayer();

            if ( parameters.IsEmpty )
            {
                ShowUsage( source );
            }
            else if ( parameters[0].Is( "all" ) )
            {
                player.RocketPlayer.Inventory.Items.ToList().ForEach( item => Repair( player, item ) );
                EssLang.ALL_REPAIRED.SendTo( source );
            }
            else if ( parameters[0].Is( "hand" ) )
            {
                Repair( player, player.Inventory.Items[0] );
                Repair( player, player.Inventory.Items[1] );
                EssLang.HAND_REPAIRED.SendTo( source );
            }
        }
    }
}
