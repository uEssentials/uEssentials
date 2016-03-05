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

using Essentials.I18n;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "spawnitem",
        Aliases = new [] { "dropitem" },
        Description = "Spawn an item at given position",
        Usage = "[item] <amount> <x> <y> <z>"
    )]
    public class CommandSpawnItem : EssCommand
    {
        /*
            TODO ERROR

            >spawnitem maplestrike 1 255 255 255
            [uEssentials] System.InvalidOperationException: Cannot convert Console to Player
              at Essentials.Common.Preconditions.CheckState (Boolean expr, System.String message, System.Object[] messageArgs) [0x00000] in <filename unknown>:0
              at Essentials.Api.Command.Source.CommandSourceExtension.ToPlayer (ICommandSource src) [0x00000] in <filename unknown>:0
              at Essentials.Commands.CommandSpawnItem.OnExecute (ICommandSource source, ICommandArgs parameters) [0x00000] in <filename unknown>:0
              at Essentials.Core.Command.CommandAdapter.execute (CSteamID executorId, System.String parameter) [0x00000] in <filename unknown>:0
        */

        public override CommandResult OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.IsEmpty || (parameters.Length < 5 && source.IsConsole) )
            {
                return CommandResult.ShowUsage();
            }

            var rawId = parameters[0].ToString();
            var rawAmount = parameters.Length >= 2 ? parameters[1].ToString() : "1";
            var pos = source.ToPlayer().Position;

            if ( parameters.Length == 5 )
            {
                var argPos = parameters.GetVector3( 2 );

                if ( !argPos.HasValue )
                {
                    return CommandResult.Lang( EssLang.INVALID_COORDS, parameters[2], parameters[3], parameters[4] );
                }

                pos = argPos.Value;
            }

            ushort amount;

            if ( !ushort.TryParse( rawAmount, out amount ) )
            {
                return CommandResult.Lang( EssLang.INVALID_NUMBER, rawAmount );
            }

            var itemAsset = ItemUtil.GetItem( rawId );

            if ( itemAsset.IsAbsent )
            {
                return CommandResult.Lang( EssLang.INVALID_ITEM_ID, rawId );
            }

            var item = new Item( itemAsset.Value.id, true );

            if ( itemAsset.Value is ItemFuelAsset )
            {
                item.Metadata[0] = 1;
            }

            for ( var i = 0; i < amount; i++ )
            {
                ItemManager.dropItem( item, pos, true, true, true );
            }

            if ( parameters.Length == 5 )
                EssLang.SPAWNED_ITEM_AT.SendTo( source, amount, itemAsset.Value.Name, pos.x, pos.y, pos.z );
            else
                EssLang.SPAWNED_ITEM.SendTo( source, amount, itemAsset.Value.Name );

            return CommandResult.Success();
        }
    }
}
