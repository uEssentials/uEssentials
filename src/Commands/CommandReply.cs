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
using static Essentials.Commands.CommandTell;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "reply",
        Aliases = new[] {"r"},
        Description = "Reply an message",
        AllowedSource = AllowedSource.PLAYER,
        Usage = "[message]"
    )]
    public class CommandReply : EssCommand
    {
        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.Length == 0 )
            {
                ShowUsage( source );
            }
            else
            {
                var target = ( from conversation
                               in Conversations
                               where conversation.Value.Equals( source.DisplayName )
                               select UPlayer.From( conversation.Key )
                              ).FirstOrDefault();

                if ( target == null )
                {
                    EssLang.NO_LONGER_ONLINE.SendTo( source );
                    return;
                }

                source.DispatchCommand( $"tell {target.DisplayName} {parameters.GetJoinedArguments( 0 )}" );
            }
        }
    }
}