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
using System.Linq;
using System.Text;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Core;
using Essentials.Core.Command;
using Essentials.I18n;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "essentials",
        Description = "View plugin informations.",
        Aliases = new[] { "ess", "?", "uessentials" }
    )]
    public class CommandEssentials : EssCommand
    {
        private string _cachedCommands;

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.IsEmpty )
            {
                source.SendMessage( "Use /ess <commands/help/info>" );
            }
            else
            {
                switch ( parameters[0].ToString().ToLower() )
                {
                    case "commands":
                        if ( source.IsConsole )
                        {
                            if ( _cachedCommands == null )
                            {
                                var builder = new StringBuilder( "Commands: \n" );

                                Func<ICommand, bool> isEssentialsCommand = cmd =>
                                {
                                    var asm = cmd.GetType().Assembly;

                                    if ( cmd.GetType() == typeof (MethodCommand) )
                                    {
                                        asm = ((MethodCommand) cmd).Owner.Assembly;
                                    }

                                    return asm.Equals( typeof(EssCore).Assembly );
                                };

                                EssProvider.CommandManager.Commands
                                    .Where( isEssentialsCommand )
                                    .ForEach( command => {
                                        builder.Append( "  /" )
                                               .Append( command.Name.ToLower() )
                                               .Append( command.Usage == "" ? "" : " " + command.Usage )
                                               .Append( " - " ).Append( command.Description )
                                               .AppendLine();
                                    } );

                                _cachedCommands = builder.ToString();
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine( _cachedCommands );
                            Console.WriteLine( "Use /ess help <command> to view help page." );
                        }
                        else
                        {
                            EssLang.PLAYER_CANNOT_EXECUTE.SendTo( source );
                        }
                        break;

                    case "info":
                        if ( source.IsConsole )
                            Console.ForegroundColor = ConsoleColor.Green;

                        source.SendMessage( "Author:  leonardosc", Color.yellow );
                        source.SendMessage( "Skype:   devleeo", Color.yellow );
                        source.SendMessage( "Github:  github.com/leonardosnt", Color.yellow );
                        source.SendMessage( "Version: " + EssCore.PLUGIN_VERSION, Color.yellow );
                        break;

                    case "help":
                        if ( source.IsConsole )
                            Console.ForegroundColor = ConsoleColor.Green;

                        if ( parameters.Length == 1 )
                        {
                            source.SendMessage( "Use /ess help <command>" );
                        }
                        else
                        {
                            var command = EssProvider.CommandManager.GetByName( parameters[1].ToString() );

                            if ( command == null )
                            {
                                source.SendMessage( $"Command {parameters[1]} does not exists", Color.red );
                            }
                            else
                            {
                                source.SendMessage( "Command: " + command.Name , Color.cyan);
                                source.SendMessage( "  Arguments: <optional> [required]", Color.cyan );
                                source.SendMessage( "  Help: " + command.Description, Color.cyan );
                                source.SendMessage( "  Usage: /" + command.Name + " " + command.Usage, Color.cyan );
                                if ( command.Aliases.Any() )
                                    source.SendMessage( "  Aliases: " + string.Join( ", ", command.Aliases ), Color.cyan );
                            }
                        }
                        break;

                    default:
                        source.SendMessage( "Use /ess <commands/help/info>" );
                        break;
                }
            }
        }
    }
}
