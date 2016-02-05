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
using Essentials.Core.Components;
using Essentials.I18n;

namespace Essentials.Commands
{
    public class CommandItemFeatures
    {
        [CommandInfo( 
            Name = "autoreload",
            Usage = "[on|off]",
            Description = "Auto reload weapon when ammo is less than 5",
            AllowedSource = AllowedSource.PLAYER
        )]
        public void NeverReloadCommand( ICommandSource src, ICommandArgs args, ICommand cmd )
        {
            var player = src.ToPlayer();

            if ( args.IsEmpty )
                goto usage;

            switch ( args[0].ToLowerString )
            {
                case "on":
                case "1":
                    var wFeature = player.GetComponent<ItemFeatures>() ?? player.AddComponent<ItemFeatures>();
                    wFeature.AutoReload = true;
                    EssLang.AUTO_RELOAD_ENABLED.SendTo( src );
                    return;

                case "off":
                case "0":
                    wFeature = player.GetComponent<ItemFeatures>() ?? player.AddComponent<ItemFeatures>();
                    wFeature.AutoReload = false;
                    EssLang.AUTO_RELOAD_DISABLED.SendTo( src );
                    return;

                default:
                    goto usage;
            }

            usage:
            src.SendMessage( $"Use /{cmd.Name} {cmd.Usage}" );
        }

        [CommandInfo( 
            Name = "autorepair",
            Usage = "[on|off]",
            Description = "Auto repair weapon when quality is less than 90",
            AllowedSource = AllowedSource.PLAYER
        )]
        public void NeverRepairCommand( ICommandSource src, ICommandArgs args, ICommand cmd )
        {
            var player = src.ToPlayer();

            if ( args.IsEmpty )
                goto usage;

            switch ( args[0].ToLowerString )
            {
                case "on":
                case "1":
                    var wFeature = player.GetComponent<ItemFeatures>() ?? player.AddComponent<ItemFeatures>();
                    wFeature.AutoRepair = true;
                    EssLang.AUTO_REPAIR_ENABLED.SendTo( src );
                    return;

                case "off":
                case "0":
                    wFeature = player.GetComponent<ItemFeatures>() ?? player.AddComponent<ItemFeatures>();
                    wFeature.AutoRepair = false;
                    EssLang.AUTO_REPAIR_DISABLED.SendTo( src );
                    return;

                default:
                    goto usage;
            }

            usage:
            src.SendMessage( $"Use /{cmd.Name} {cmd.Usage}" );
        }
    }
}