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

using System.Collections.Generic;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;

namespace Essentials.Commands
{
    //TODO: spy
    [CommandInfo(
        Name = "tell",
        Aliases = new[] {"msg", "pm"},
        Description = "Send private message to an player",
        Usage = "[player] [message]"
    )]
    public class CommandTell : EssCommand
    {
        internal static readonly Dictionary<string, string> Conversations = new Dictionary<string, string>();

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.Length < 2 )
            {
                ShowUsage( source );
            }
            else
            {
                var target = parameters[0].ToPlayer;

                if ( target == null )
                {
                    EssLang.PLAYER_NOT_FOUND.SendTo( source, parameters[0] );
                }
                else
                {
                    var message = string.Format(
                        EssProvider.Config.PrivateMessageFormat,
                        source.DisplayName,
                        parameters.Join( 1 )
                    );

                    target.SendMessage( message );

                    if ( Conversations.ContainsKey( source.DisplayName ) )
                    {
                        if ( !Conversations[ source.DisplayName ].Equals( target.DisplayName ) )
                        {
                            Conversations[ source.DisplayName ] = target.CharacterName;
                        }
                    }
                    else
                    {
                        Conversations.Add( source.DisplayName, target.DisplayName );
                    }
                }
            }
        }
    }
}
