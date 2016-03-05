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


using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Task;
using Essentials.I18n;

namespace Essentials.InternalModules.Warp.Commands
{
    [CommandInfo(
        Name = "warp",
        Description = "Teleport you to given warp.",
        AllowedSource = AllowedSource.PLAYER,
        Usage = "[warp_name]"
    )]
    public class CommandWarp : EssCommand
    {
        public override CommandResult OnExecute ( ICommandSource source, ICommandArgs parameters )
        {
            var player = source.ToPlayer();

            if ( parameters.Length == 0 || parameters.Length > 1 )
            {
                return CommandResult.ShowUsage();
            }

            if ( !WarpModule.Instance.WarpManager.Contains( parameters[0].ToString() ) )
            {
                return CommandResult.Lang( EssLang.WARP_NOT_EXIST, parameters[0] );
            }

            if ( !player.HasPermission( $"essentials.warp.{parameters[0]}" ) )
            {
                return CommandResult.Lang( EssLang.WARP_NO_PERMISSION, parameters[0] );
            }

            var dest = WarpModule.Instance.WarpManager[parameters[0].ToString()];
            var cooldown = EssProvider.Config.WarpCooldown;

            if ( cooldown > 0 && !player.HasPermission( "essentials.bypass.warpcooldown" ) )
            {
                EssLang.WARP_COOLDOWN.SendTo( source, cooldown );
            }

            Tasks.New( t =>
            {
                player.Teleport( dest.Location, dest.Rotation );
                EssLang.WARP_TELEPORTED.SendTo( source, parameters[0] );
            }).Delay( player.HasPermission( "essentials.bypass.warpcooldown" ) ? 0 : cooldown * 1000 ).Go();

            return CommandResult.Success();
        }
    }
}
