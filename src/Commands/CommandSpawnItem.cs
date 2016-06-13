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
using UnityEngine;
using Essentials.Api;

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
        public override CommandResult OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.IsEmpty || (args.Length < 5 && src.IsConsole) )
            {
                return CommandResult.ShowUsage();
            }

            var rawId = args[0].ToString();
            var rawAmount = args.Length >= 2 ? args[1].ToString() : "1";
            Vector3 pos;

            if ( args.Length == 5 )
            {
                var argPos = args.GetVector3( 2 );

                if ( !argPos.HasValue )
                {
                    return CommandResult.Lang( EssLang.INVALID_COORDS, args[2], args[3], args[4] );
                }

                pos = argPos.Value;
            }
            else
            {
                pos = src.ToPlayer().Position;
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

            if ( UEssentials.Config.GiveItemBlacklist.Contains( itemAsset.Value.id ) &&
                 !src.HasPermission( "essentials.bypass.blacklist.item" ) )
            {
                return CommandResult.Lang( EssLang.BLACKLISTED_ITEM,  $"{itemAsset.Value.itemName} ({itemAsset.Value.Id})" );
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

            if ( args.Length == 5 )
                EssLang.SPAWNED_ITEM_AT.SendTo( src, amount, itemAsset.Value.Name, pos.x, pos.y, pos.z );
            else
                EssLang.SPAWNED_ITEM.SendTo( src, amount, itemAsset.Value.Name );

            return CommandResult.Success();
        }
    }
}
