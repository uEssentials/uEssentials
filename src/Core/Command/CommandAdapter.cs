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
using Essentials.I18n;
using Rocket.API;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;

namespace Essentials.Core.Command
{
    internal class CommandAdapter : Rocket.API.IRocketCommand
    {
        public List<string>     Aliases         { get; }
        public AllowedCaller    AllowedCaller   { get; }
        public string           Help            { get; }
        public string           Name            { get; private set; }
        public List<string>     Permissions     { get; }
        public string           Syntax          { get; }
        
        internal readonly ICommand Command;

        internal CommandAdapter( ICommand command )
        {
            Command         = command;
            Name            = command.Name;
            Aliases         = command.Aliases.ToList();
            Help            = command.Description;
            Syntax          = command.Usage;
            Permissions     = new List<string>(1) { command.Permission };
            AllowedCaller   = AllowedCaller.Both;
        } 

        public void Execute( IRocketPlayer caller, string[] args )
        {
            try
            {
                var commandSource = caller is UnturnedPlayer
                                    ? UPlayer.From( (UnturnedPlayer) caller )
                                    : UEssentials.ConsoleSource;

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
                    var cmdArgs  = new CommandArgs( args );
                    var preExec = new CommandPreExecuteEvent( Command, cmdArgs, commandSource );
                    EssentialsEvents._OnCommandPreExecute?.Invoke( preExec );

                    if ( preExec.Cancelled )
                    {
                        return;
                    }

                    var result = Command.OnExecute( commandSource , cmdArgs );

                    if ( result == null ) return;

                    if ( result.Type == CommandResult.ResultType.SHOW_USAGE )
                    {
                        commandSource.SendMessage( $"Use /{Command.Name} {Command.Usage}" );
                    }
                    else if ( result.Message != null )
                    {
                        var message = result.Message;

                        var color = ColorUtil.GetColorFromString( ref message );

                        commandSource.SendMessage( message, color );
                    }

                    var posExec = new CommandPosExecuteEvent( Command, cmdArgs, commandSource, result );
                    EssentialsEvents._OnCommandPosExecute?.Invoke( posExec );
                }
            }
            catch ( Exception e )
            {
                if ( caller is UnturnedPlayer )
                    UPlayer.TryGet( (UnturnedPlayer) caller, EssLang.COMMAND_ERROR_OCURRED.SendTo );

                UEssentials.Logger.LogError( e.ToString() );
            }
        }

        internal class CommandAliasAdapter : CommandAdapter
        {
            internal CommandAliasAdapter( ICommand command, string alias ) : base(command)
            {
                base.Name = alias;
            }
        }
    }
}