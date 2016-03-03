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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;
using Essentials.InternalModules.Kit.Item;
using UnityEngine;
using Essentials.Common;
using SDG.Unturned;

namespace Essentials.InternalModules.Kit.Commands
{
    [CommandInfo(
        Name = "dropkit",
        Description = "Drop an kit at given player/position",
        Usage = "[kit] <player | x y z>"
    )]
    public class CommandDropKit : EssCommand
    {
        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            switch (args.Length)
            {
                case 1:
                    if ( src.IsConsole )
                    {
                        goto usage;
                    }

                    DropKit( src, args[0], src.ToPlayer().Position );
                    EssLang.DROPKIT_SENDER.SendTo( src );
                    return;

                case 2:
                    var found = UPlayer.TryGet( args[1], player => {
                        DropKit( src, args[0], player.Position );
                        EssLang.DROPKIT_PLAYER.SendTo( src, player.DisplayName );
                    } );

                    if ( !found )
                    {
                        EssLang.PLAYER_NOT_FOUND.SendTo( src, args[1] );
                    }
                    return;

                case 4:
                    var pos = args.GetVector3( 1 );

                    if ( pos.HasValue )
                    {
                        DropKit( src, args[0], pos.Value );
                        EssLang.DROPKIT_LOCATION.SendTo( src, args[1], args[2], args[3] );
                    }
                    else
                    {
                        EssLang.INVALID_COORDS.SendTo( src, args[1], args[2], args[3] );    
                    }
                    return;

                default:
                    goto usage;
            }

            usage:
            ShowUsage( src );
        }

        public static void DropKit( ICommandSource src, ICommandArgument kitArg, Vector3 pos )
        {
            var kitManager = KitModule.Instance.KitManager;
            var kitName = kitArg.ToString();

            if ( !kitManager.Contains( kitName ) )
            {
                EssLang.KIT_NOT_EXIST.SendTo( src, kitName );
            }
            else
            {
                var kitItems = kitManager.GetByName( kitName ).Items;

                kitItems.Where( i => i is KitItem ).Cast<KitItem>().ForEach( i =>
                    ItemManager.dropItem( i.UnturnedItem, pos, true, true, true )
                );
            }
        }
    }
}
