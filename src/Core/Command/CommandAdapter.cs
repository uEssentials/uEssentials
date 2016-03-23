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

using System;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Events;
using Essentials.Api.Unturned;
using Essentials.Common.Util;
using Essentials.Event;
using Steamworks;
using Essentials.I18n;

namespace Essentials.Core.Command
{
    internal class CommandAdapter : SDG.Unturned.Command
    {
        internal readonly ICommand Command;

        internal CommandAdapter( ICommand command )
        {
            Command = command;
            _help = command.Description;
            _info = command.Name + " " + command.Usage;
            _command = command.Name;
        }

        protected override void execute( CSteamID executorId, string parameter )
        {
            try
            {
                var commandSource = executorId == CSteamID.Nil
                                    ? EssProvider.ConsoleSource
                                    : UPlayer.From( executorId );

                if ( commandSource.IsConsole && Command.AllowedSource == AllowedSource.PLAYER ) 
                {
                    EssLang.CONSOLE_CANNOT_EXECUTE.SendTo( commandSource );
                }
                else if ( !commandSource.IsConsole && Command.AllowedSource == AllowedSource.CONSOLE ) 
                {
                    EssLang.PLAYER_CANNOT_EXECUTE.SendTo( commandSource );    
                }
                else
                {
                    var preExec = new CommandPreExecuteEvent( Command, commandSource );
                    EssentialsEvents._OnCommandPreExecute?.Invoke( preExec );

                    if ( preExec.Cancelled )
                    {
                        return;
                    }

                    var result = Command.OnExecute( commandSource , new CommandArgs( parameter ) );

                    if ( result == null ) return;

                    if ( result.Type == CommandResult.ResultType.SHOW_USAGE )
                    {
                        commandSource.SendMessage( $"Use /{Command.Name} {Command.Usage}" );
                    }
                    else if ( result.Message != null )
                    {
                        var message = result.Message;

                        var color = ColorUtil.GetMessageColor( ref message );

                        commandSource.SendMessage( message, color );
                    }

                    var posExec = new CommandPosExecuteEvent( Command, commandSource, result );
                    EssentialsEvents._OnCommandPosExecute?.Invoke( posExec );
;                }
            }
            catch ( Exception e )
            {
                UPlayer.TryGet( executorId, EssLang.COMMAND_ERROR_OCURRED.SendTo );

                EssProvider.Logger.LogError( e.ToString() );
            }
        }

        internal class CommandAliasAdapter : CommandAdapter
        {
            internal CommandAliasAdapter( ICommand command, string alias ) : base(command)
            {
                _help = command.Description;
                _info = alias + " " + command.Usage;
                _command = alias;
            }
        }
    }
}
