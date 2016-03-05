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
using Essentials.Api.Unturned;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "experience",
        Aliases = new[] { "exp" },
        Description = "Give experience to you/player",
        Usage = "[give|take] [amount] <target>"
    )]
    public class CommandExperience : EssCommand
    {
        public override CommandResult OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( (!src.IsConsole && args.Length < 2) || (src.IsConsole && args.Length < 3) )
                return CommandResult.ShowUsage();

            UPlayer target;

            if ( args.Length == 3 )
            {
                target = args[2].ToPlayer;
            }
            else if ( src.IsConsole )
            {
                return CommandResult.ShowUsage();
            }
            else
            {
                target = src.ToPlayer();
            }
            
            if ( target == null )
            {
                return CommandResult.Lang( EssLang.PLAYER_NOT_FOUND, args[2] );
            }

            if ( !args[1].IsInt )
            {
                return CommandResult.Lang( EssLang.INVALID_NUMBER, args[1] );
            }

            if ( args[1].ToInt < 0 )
            {
                return CommandResult.Lang( EssLang.NEGATIVE_OR_LARGE );
            }

            var amount = (uint) args[1].ToInt;
            var playerExp = target.UnturnedPlayer.skills.Experience;

            switch (args[0].ToLowerString)
            {
                case "take":
                    playerExp -= playerExp < amount ? playerExp : amount;

                    if ( target != src )
                        EssLang.EXPERIENCE_TAKE.SendTo( src, amount, target.DisplayName );
                    EssLang.EXPERIENCE_LOST.SendTo( target, amount );
                    break;

                case "give":
                    playerExp += amount;

                    if ( target != src )
                        EssLang.EXPERIENCE_GIVEN.SendTo( src, amount, target.DisplayName );
                    EssLang.EXPERIENCE_RECEIVED.SendTo( target, amount );
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            target.UnturnedPlayer.skills.Experience = playerExp;
            target.UnturnedPlayer.skills.askSkills( target.CSteamId );

            return CommandResult.Success();
        }
    }
}
