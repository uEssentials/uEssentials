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

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.IsEmpty )
            {
                if ( source.IsConsole )
                    ShowUsage( source );
                else
                    GiveMaxSkills( source.ToPlayer(), false );
            }
            else
            {
                if ( !parameters[0].IsBool || parameters.Length < 2 && source.IsConsole )
                {
                    ShowUsage( source );
                    return;
                }

                var overpower = parameters[0].ToBool;
                var targetPlayer = parameters.Length == 2 ? parameters[1].ToPlayer : source.ToPlayer();

                if ( targetPlayer == null )
                    EssLang.PLAYER_NOT_FOUND.SendTo( source, parameters[1] );
                else
                {
                    GiveMaxSkills( targetPlayer, overpower );

                    if ( source.IsConsole || source.ToPlayer() != targetPlayer )
                        EssLang.MAX_SKILLS_TARGET.SendTo( source, targetPlayer.DisplayName );
                }
            }
        }
    }
}
