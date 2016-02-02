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

using UnityEngine;
using Essentials.I18n;
using System;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "spawnitem",
        Aliases = new [] { "dropitem" },
        Description = "Spawn an item at given position",
        Usage = "[id] <amount> <x> <y> <z>"
    )]
    public class CommandSpawnItem : EssCommand
    {
        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.IsEmpty || ( parameters.Length < 5 && source.IsConsole ) )
            {
                ShowUsage( source );
            }
            else
            {
                var rawId          = parameters[0].ToString();
                var rawAmount      = parameters.Length >= 2 ? parameters[1].ToString() : "1";
                var pos            = source.ToPlayer().Position;

                if ( parameters.Length == 5 )
                {
                    try
                    {
                        var x = (float) parameters[2].ToDouble;
                        var y = (float) parameters[3].ToDouble;
                        var z = (float) parameters[4].ToDouble;
                        pos = new Vector3( x, y, z );
                    }
                    catch ( FormatException )
                    {
                        EssLang.INVALID_COORDS.SendTo( source, parameters[2], parameters[3], parameters[4] );
                        return;
                    }
                }

                ushort id;
                byte amount;

                if ( !ushort.TryParse( rawId, out id ) )
                {
                    EssLang.INVALID_NUMBER.SendTo( source, rawId );
                    return;
                }

                if ( !byte.TryParse( rawAmount, out amount ) )
                {
                    EssLang.INVALID_NUMBER.SendTo( source, rawAmount );
                    return;
                }

                var item = new Item(id, 1, 100);
                var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, id);

                if ( itemAsset == null )
                {
                    EssLang.INVALID_ITEM_ID.SendTo( source, id );
                    return;
                }

                for ( var i = 0; i < amount; i++ ) 
                {
                    ItemManager.dropItem( item, pos, true, true, true );
                }

                if ( parameters.Length == 5 )
                    EssLang.SPAWNED_ITEM_AT.SendTo( source, amount, itemAsset.Name, pos.x, pos.y, pos.z);
                else
                    EssLang.SPAWNED_ITEM.SendTo( source, amount, itemAsset.Name );
            }
        }
    }
}
