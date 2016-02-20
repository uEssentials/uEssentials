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
using Essentials.I18n;
using System;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "maxskills",
        Description = "Set to max level all of your/player skills",
        Usage = "<overpower[true|false]> <player>"
    )]
    public class CommandMaxSkills : EssCommand
    {
        private static readonly Action<UPlayer, bool> GiveMaxSkills = ( player, overpower ) =>
        {
            var pSkills = player.UnturnedPlayer.skills;

            foreach ( var skill in pSkills.skills.SelectMany( skArr => skArr ) )
            {
                skill.level = overpower ? byte.MaxValue : skill.max;
            }

            pSkills.askSkills( player.CSteamId );
            EssLang.MAX_SKILLS.SendTo(player);
        };

        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.IsEmpty )
            {
                if ( src.IsConsole )
                {
                    ShowUsage( src );
                }
                else
                {
                    GiveMaxSkills( src.ToPlayer(), false );
                }
            }
            else
            {
                if ( args.Length < 2 && src.IsConsole )
                {
                    ShowUsage( src );
                    return;
                }

                if ( !args[0].IsBool )
                {
                    EssLang.INVALID_BOOLEAN.SendTo( src, args[0] );
                    return;
                }

                if ( args.Length == 2 && !src.HasPermission( Permission + ".other" ) )
                {
                    EssLang.COMMAND_NO_PERMISSION.SendTo( src );
                    return;
                }

                var overpower = args[0].ToBool;
                var targetPlayer = args.Length == 2 ? args[1].ToPlayer : src.ToPlayer();

                if ( targetPlayer == null )
                {
                    EssLang.PLAYER_NOT_FOUND.SendTo( src, args[1] );
                }
                else
                {
                    GiveMaxSkills( targetPlayer, overpower );

                    if ( src.IsConsole || src.ToPlayer() != targetPlayer )
                    {
                        EssLang.MAX_SKILLS_TARGET.SendTo( src, targetPlayer.DisplayName );
                    }
                }
            }
        }
    }
}
